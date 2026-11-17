using MediCare.Application.Abstractions;
using MediCare.Application.Modules.Assistant.Commands.Chat;
using MediCare.Application.Modules.Statistics.Queries.Dashboard;
using MediCare.Domain.Entities.HospitalRecords;
using MediCare.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediCare.Tests.UnitTests;

/// <summary>
/// What the external AI service receives. A fake IAiChatClient captures every outgoing message,
/// then the tests check that no patient name, e-mail, phone number or medical note is in it.
/// </summary>
public class AssistantPrivacyTests
{
    private const string FirstName = "Amra";
    private const string LastName = "Hodžić";
    private const string PatientEmail = "amra.hodzic@demo.medicare.com";
    private const string Phone = "061 123 456";
    private const string Note = "Dijabetes tip 2, uzima insulin";
    private static readonly string[] Secrets = [FirstName, LastName, PatientEmail, Phone, Note, "insulin"];

    /// <summary>Plays back prepared model replies and keeps everything that would be sent to the service.</summary>
    private sealed class CapturingAiClient(params AiChatResult[] replies) : IAiChatClient
    {
        private int _next;
        private readonly List<string> _outgoing = [];

        /// <summary>All messages of all requests as one text (role, content, tool call arguments, tool list).</summary>
        public string Outgoing => string.Join("\n", _outgoing);

        /// <summary>The most recent tool result the model received.</summary>
        public string LastToolResult { get; private set; } = string.Empty;

        public Task<AiChatResult> CompleteAsync(IReadOnlyList<AiMessage> messages, IReadOnlyList<AiTool> tools, CancellationToken ct = default)
        {
            foreach (var m in messages)
            {
                _outgoing.Add($"{m.Role}: {m.Content} {string.Join(' ', (m.ToolCalls ?? []).Select(c => c.Name + c.ArgumentsJson))}");
                if (m.Role == AiRoles.Tool) LastToolResult = m.Content ?? string.Empty;
            }
            _outgoing.AddRange(tools.Select(t => $"tool: {t.Name} {t.Description} {t.ParametersJsonSchema}"));
            return Task.FromResult(replies[Math.Min(_next++, replies.Length - 1)]);
        }
    }

    private sealed class AdminUser : IAppCurrentUser
    {
        public int? UserId => 1;
        public string? Email => null;
        public bool IsAuthenticated => true;
        public bool IsAdmin => true;
        public bool IsManager => false;
        public bool IsEmployee => false;
    }

    private static AiChatResult Call(string name, string args) => new(null, [new AiToolCall(Guid.NewGuid().ToString(), name, args)]);
    private static AiChatResult Answer(string text) => new(text, []);

    private static AssistantChatCommand Ask(params (string Role, string Content)[] messages) =>
        new() { Messages = messages.Select(m => new AssistantChatMessageDto { Role = m.Role, Content = m.Content }).ToList() };

    /// <summary>A real MediatR sender, so the statistics tool runs the real statistics handler.</summary>
    private static ISender RealSender(DatabaseContext db) =>
        new ServiceCollection()
            .AddSingleton<IAppDbContext>(db)
            .AddMediatR(c => c.RegisterServicesFromAssembly(typeof(StatisticsDashboardQuery).Assembly))
            .BuildServiceProvider()
            .GetRequiredService<ISender>();

    private static AssistantChatCommandHandler AdminHandler(DatabaseContext db, IAiChatClient ai) =>
        new(db, new AdminUser(), ai, RealSender(db), new SqlMedicineSearchService(db), NullLogger<AssistantChatCommandHandler>.Instance);

    /// <summary>One patient with name, e-mail and phone, a booked treatment with a medical note, and a paid order.</summary>
    private static async Task SeedPatientDataAsync(DatabaseContext db)
    {
        var confirmed = new OrderStatus { StatusName = "CONFIRMED" };
        var paid = new OrderStatus { StatusName = "PAID" };
        db.OrderStatus.AddRange(confirmed, paid);
        var treatment = new Treatments { ServiceName = "Ortopedija", Description = "Bol u leđima", ImagePath = "x", Price = 70m, TreatmentCategoryId = 1, isEnabled = true };
        db.Treatments.Add(treatment);
        var patient = new Users
        {
            FirstName = FirstName, LastName = LastName, Email = PatientEmail, PhoneNumber = Phone,
            UserName = "amra", City = "Mostar", Adress = "Titova 1", PasswordHash = "x"
        };
        db.Users.Add(patient);
        await db.SaveChangesAsync();

        db.Reservations.Add(new Reservations
        {
            UserId = patient.Id, TreatmentId = treatment.Id, ReservationDate = DateTime.Today.AddDays(2),
            ReservationTime = new TimeSpan(10, 30, 0), OrderStatusId = confirmed.Id, Price = 70m, Notes = Note
        });
        db.Orders.Add(new Orders { UserId = patient.Id, OrderDate = DateTime.Today, TotalPrice = 25m, OrderStatusId = paid.Id });
        await db.SaveChangesAsync();
    }

    private static void AssertNoPersonalData(string text)
    {
        foreach (var secret in Secrets)
            Assert.DoesNotContain(secret, text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReservationQuery_OutgoingMessagesHaveNoNameEmailPhoneOrNote_ButAdminStillGetsThemInTheTable()
    {
        // Arrange
        using var db = TestDb.Create();
        await SeedPatientDataAsync(db);
        var ai = new CapturingAiClient(Call("get_reservations", "{}"), Answer("Imate 1 rezervaciju za ortopediju u 10:30."));

        // Act
        var result = await AdminHandler(db, ai).Handle(Ask(("user", "Ko je rezervisan u narednih 7 dana?")), CancellationToken.None);

        // Assert – the external service got dates, times, treatments and statuses ...
        Assert.Contains("Ortopedija", ai.LastToolResult);
        Assert.Contains("10:30", ai.LastToolResult);
        Assert.Contains("CONFIRMED", ai.LastToolResult);
        // ... but no personal data in ANY outgoing message (system prompt, history, tool results)
        AssertNoPersonalData(ai.Outgoing);

        // The admin still sees names and notes: the app attaches them to the answer itself
        var table = Assert.Single(result.Details);
        var row = Assert.Single(table.Rows);
        Assert.Contains($"{FirstName} {LastName}", row);
        Assert.Contains(Note, row);
    }

    [Fact]
    public async Task OrdersQuery_OutgoingMessagesHaveNoCustomerName()
    {
        using var db = TestDb.Create();
        await SeedPatientDataAsync(db);
        var ai = new CapturingAiClient(Call("get_orders", "{}"), Answer("Imate 1 narudžbu."));

        var result = await AdminHandler(db, ai).Handle(Ask(("user", "Koje su zadnje narudžbe?")), CancellationToken.None);

        AssertNoPersonalData(ai.Outgoing);
        Assert.Contains("PAID", ai.LastToolResult);
        Assert.Contains($"{FirstName} {LastName}", Assert.Single(Assert.Single(result.Details).Rows));
    }

    [Fact]
    public async Task StatisticsQuery_OutgoingMessagesHaveNoPersonalData_AndKeepTheNumbers()
    {
        using var db = TestDb.Create();
        await SeedPatientDataAsync(db);
        var ai = new CapturingAiClient(Call("get_statistics", "{}"), Answer("Prihod je 25 KM."));

        await AdminHandler(db, ai).Handle(Ask(("user", "Kakva je statistika?")), CancellationToken.None);

        AssertNoPersonalData(ai.Outgoing);
        Assert.Contains("\"ordersRevenue\":25", ai.LastToolResult); // aggregates are still sent
        Assert.Contains("\"ordersCount\":1", ai.LastToolResult);
    }

    [Fact]
    public async Task ChatHistory_SecondQuestion_StillSendsNoPersonalData()
    {
        // Arrange – first turn: reservation query, the answer is kept by the browser as history
        using var db = TestDb.Create();
        await SeedPatientDataAsync(db);
        var firstAi = new CapturingAiClient(Call("get_reservations", "{}"), Answer("Ima 1 rezervacija (10:30, ortopedija)."));
        var first = await AdminHandler(db, firstAi).Handle(Ask(("user", "Rezervacije?")), CancellationToken.None);

        // Act – second turn: history = question + the earlier answer + a new question
        var secondAi = new CapturingAiClient(Answer("Ok."));
        await AdminHandler(db, secondAi).Handle(
            Ask(("user", "Rezervacije?"), ("assistant", first.Reply), ("user", "A koliko ih je ukupno?")),
            CancellationToken.None);

        // Assert – the earlier answer in the history is only the model's text, never the table with names
        AssertNoPersonalData(secondAi.Outgoing);
        Assert.Contains(first.Reply, secondAi.Outgoing);
    }

    [Fact]
    public async Task SystemPrompt_DoesNotContainTheUsersName()
    {
        using var db = TestDb.Create();
        await SeedPatientDataAsync(db);
        // the logged-in admin (id 1) is the seeded patient, so a name in the prompt would show up here
        var ai = new CapturingAiClient(Answer("Zdravo."));

        await AdminHandler(db, ai).Handle(Ask(("user", "Zdravo")), CancellationToken.None);

        AssertNoPersonalData(ai.Outgoing);
    }
}

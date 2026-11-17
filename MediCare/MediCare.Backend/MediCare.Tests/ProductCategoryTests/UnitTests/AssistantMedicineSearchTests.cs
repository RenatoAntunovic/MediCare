using MediCare.Application.Abstractions;
using MediCare.Application.Modules.Assistant.Commands.Chat;
using MediCare.Domain.Entities.HospitalRecords;
using MediCare.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediCare.Tests.UnitTests;

/// <summary>search_medicines must search the whole active catalogue first and limit the result list afterwards.</summary>
public class AssistantMedicineSearchTests
{
    private sealed class OneToolCallClient(string query) : IAiChatClient
    {
        private bool _called;
        public string ToolResult { get; private set; } = string.Empty;

        public Task<AiChatResult> CompleteAsync(IReadOnlyList<AiMessage> messages, IReadOnlyList<AiTool> tools, CancellationToken ct = default)
        {
            if (!_called)
            {
                _called = true;
                return Task.FromResult(new AiChatResult(null, [new AiToolCall("1", "search_medicines", $$"""{"query":"{{query}}"}""")]));
            }
            ToolResult = messages.Last().Content ?? string.Empty;
            return Task.FromResult(new AiChatResult("ok", []));
        }
    }

    private sealed class PatientUser : IAppCurrentUser
    {
        public int? UserId => 1;
        public string? Email => null;
        public bool IsAuthenticated => true;
        public bool IsAdmin => false;
        public bool IsManager => false;
        public bool IsEmployee => true;
    }

    /// <summary>250 active medicines "Med001" … "Med250" (alphabetical = numeric order) and one disabled "Med999".</summary>
    private static async Task<DatabaseContext> CatalogueOf250Async()
    {
        var db = TestDb.Create();
        db.MedicineCategories.Add(new MedicineCategories { Id = 1, Name = "Tablete", IsEnabled = true });
        await db.SaveChangesAsync();

        db.Medicine.AddRange(Enumerable.Range(1, 250).Select(i => new Medicine
        {
            Name = $"Med{i:000}", Description = "Lijek", ImagePath = "x", Price = 5m, Weight = 10, MedicineCategoryId = 1, isEnabled = true
        }));
        db.Medicine.Add(new Medicine { Name = "Med999", Description = "Lijek", ImagePath = "x", Price = 5m, Weight = 10, MedicineCategoryId = 1, isEnabled = false });
        await db.SaveChangesAsync();
        return db;
    }

    private static async Task<string> SearchAsync(DatabaseContext db, string query)
    {
        var ai = new OneToolCallClient(query);
        var handler = new AssistantChatCommandHandler(
            db, new PatientUser(), ai, null!, new SqlMedicineSearchService(db), NullLogger<AssistantChatCommandHandler>.Instance);
        await handler.Handle(new AssistantChatCommand { Messages = [new AssistantChatMessageDto { Role = "user", Content = "q" }] }, CancellationToken.None);
        return ai.ToolResult;
    }

    [Fact]
    public async Task Search_MedicineAfterTheFirst200_IsFound_AndTheResultIsNotAPieceOfTheList()
    {
        using var db = await CatalogueOf250Async();

        var result = await SearchAsync(db, "Med230"); // number 230 of 250 – beyond the first 200 by name

        Assert.Contains("\"match\":true", result);
        Assert.Contains("Med230", result);
        Assert.DoesNotContain("Med001", result); // no unrelated part of the catalogue
    }

    [Fact]
    public async Task Search_NoMatch_ReturnsNoMatchAndNoMedicines()
    {
        using var db = await CatalogueOf250Async();

        var result = await SearchAsync(db, "Nepostojeci");

        Assert.Contains("\"match\":false", result);
        Assert.Contains("\"medicines\":[]", result);
        Assert.DoesNotContain("Med0", result); // nothing from the earlier part of the list
    }

    [Fact]
    public async Task Search_DisabledMedicine_IsNotFound()
    {
        using var db = await CatalogueOf250Async();

        var result = await SearchAsync(db, "Med999");

        Assert.Contains("\"match\":false", result);
    }

    [Fact]
    public async Task Search_KeepsTheAvailabilityDisplay()
    {
        using var db = await CatalogueOf250Async();
        var medicine = await db.Medicine.SingleAsync(m => m.Name == "Med230");
        db.Inventories.Add(new Inventories { MedicineId = medicine.Id, QuantityInStock = 0 });
        await db.SaveChangesAsync();

        var result = await SearchAsync(db, "Med230");

        Assert.Contains("\"inStock\":false", result);
    }

    [Fact]
    public async Task Search_ManyMatches_ReturnsAtMost40()
    {
        using var db = await CatalogueOf250Async();

        var result = await SearchAsync(db, "Med"); // matches all 250 – the limit comes AFTER the search

        Assert.Equal(40, System.Text.RegularExpressions.Regex.Matches(result, "\"id\":").Count);
    }
}

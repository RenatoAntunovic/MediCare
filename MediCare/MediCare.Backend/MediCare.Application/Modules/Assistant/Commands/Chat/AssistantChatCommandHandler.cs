using System.Text.Encodings.Web;
using System.Text.Json;
using MediCare.Application.Modules.Assistant.Tools;
using Microsoft.Extensions.Logging;

namespace MediCare.Application.Modules.Assistant.Commands.Chat;

/// <summary>
/// Runs the tool-calling loop: model → (tools → model)* → final answer.
/// The role (patient/admin) decides which tools and instructions the model gets.
/// </summary>
public sealed class AssistantChatCommandHandler(
    IAppDbContext db,
    IAppCurrentUser currentUser,
    IAiChatClient ai,
    ISender sender,
    ILogger<AssistantChatCommandHandler> logger)
    : IRequestHandler<AssistantChatCommand, AssistantChatResponseDto>
{
    /// <summary>Model calls per question – enough for list → slots → suggest → answer, and a safety stop for loops.</summary>
    private const int MaxRounds = 6;

    // Relaxed escaping keeps "č", "–" etc. readable – fewer tokens for the model (the JSON never goes into HTML)
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<AssistantChatResponseDto> Handle(AssistantChatCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var firstName = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FirstName)
            .FirstOrDefaultAsync(ct);

        var today = DateTime.Today;
        PatientToolbox? patientTools = null;
        IAssistantToolbox toolbox;
        string systemPrompt;

        if (currentUser.IsAdmin)
        {
            toolbox = new AdminToolbox(db, sender);
            systemPrompt = AssistantPrompts.Admin(firstName, today);
        }
        else
        {
            toolbox = patientTools = new PatientToolbox(db, userId);
            systemPrompt = AssistantPrompts.Patient(firstName, today);
        }

        var messages = new List<AiMessage> { new(AiRoles.System, systemPrompt) };
        messages.AddRange(request.Messages
            .TakeLast(AssistantChatCommand.MaxMessages)
            .Select(m => new AiMessage(m.Role, m.Content)));

        for (var round = 0; round < MaxRounds; round++)
        {
            var result = await ai.CompleteAsync(messages, toolbox.Tools, ct);

            if (result.ToolCalls.Count == 0)
            {
                return new AssistantChatResponseDto
                {
                    Reply = string.IsNullOrWhiteSpace(result.Content)
                        ? "Izvinite, nisam uspio pripremiti odgovor. Pokušajte ponovo."
                        : result.Content.Trim(),
                    Suggestions = patientTools?.Suggestions ?? new AssistantSuggestionsDto()
                };
            }

            messages.Add(new AiMessage(AiRoles.Assistant, result.Content, result.ToolCalls));

            foreach (var call in result.ToolCalls)
            {
                var output = await RunToolAsync(toolbox, call, ct);
                messages.Add(new AiMessage(AiRoles.Tool, output, ToolCallId: call.Id));
            }
        }

        logger.LogWarning("Assistant stopped after {Rounds} rounds without a final answer (user {UserId}).", MaxRounds, userId);
        return new AssistantChatResponseDto
        {
            Reply = "Izvinite, ovo pitanje mi je trenutno previše složeno. Pokušajte ga postaviti jednostavnije.",
            Suggestions = patientTools?.Suggestions ?? new AssistantSuggestionsDto()
        };
    }

    private async Task<string> RunToolAsync(IAssistantToolbox toolbox, AiToolCall call, CancellationToken ct)
    {
        try
        {
            using var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson);
            var result = await toolbox.ExecuteAsync(call.Name, args.RootElement, ct);
            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (JsonException)
        {
            return """{"error":"Arguments were not valid JSON."}""";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Assistant tool {Tool} failed.", call.Name);
            return """{"error":"The tool failed. Tell the user the data is temporarily unavailable."}""";
        }
    }
}

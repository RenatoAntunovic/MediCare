namespace MediCare.Application.Abstractions;

/// <summary>
/// Chat completion with tool (function) calling – implemented with Groq's OpenAI-compatible API.
/// Throws MediCareServiceUnavailableException when the provider is not configured or not responding.
/// </summary>
public interface IAiChatClient
{
    Task<AiChatResult> CompleteAsync(
        IReadOnlyList<AiMessage> messages,
        IReadOnlyList<AiTool> tools,
        CancellationToken ct = default);
}

public static class AiRoles
{
    public const string System = "system";
    public const string User = "user";
    public const string Assistant = "assistant";
    public const string Tool = "tool";
}

/// <summary>
/// One message in the conversation.
/// Assistant messages may carry ToolCalls; tool messages answer one call (ToolCallId).
/// </summary>
public sealed record AiMessage(
    string Role,
    string? Content,
    IReadOnlyList<AiToolCall>? ToolCalls = null,
    string? ToolCallId = null);

public sealed record AiToolCall(string Id, string Name, string ArgumentsJson);

/// <summary>A function the model may call. ParametersJsonSchema is a JSON Schema object.</summary>
public sealed record AiTool(string Name, string Description, string ParametersJsonSchema);

/// <summary>Either a final answer (Content) or a request to call tools (ToolCalls).</summary>
public sealed record AiChatResult(string? Content, IReadOnlyList<AiToolCall> ToolCalls);

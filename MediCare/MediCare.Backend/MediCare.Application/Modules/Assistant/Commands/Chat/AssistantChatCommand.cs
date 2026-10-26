namespace MediCare.Application.Modules.Assistant.Commands.Chat;

/// <summary>
/// One turn of the AI assistant chat. The client sends the conversation so far
/// (history is not stored on the server); the last message must be the user's question.
/// Patients get pharmacy/appointment help, admins get read-only answers about bookings, orders and stock.
/// </summary>
public sealed class AssistantChatCommand : IRequest<AssistantChatResponseDto>
{
    public const int MaxMessages = 20;
    public const int MaxMessageLength = 2000;

    public List<AssistantChatMessageDto> Messages { get; init; } = [];
}

public sealed class AssistantChatMessageDto
{
    /// <summary>"user" or "assistant"</summary>
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}

public sealed class AssistantChatCommandValidator : AbstractValidator<AssistantChatCommand>
{
    public AssistantChatCommandValidator()
    {
        RuleFor(x => x.Messages)
            .NotEmpty().WithMessage("Poruka je obavezna.")
            .Must(m => m.Count <= AssistantChatCommand.MaxMessages)
            .WithMessage($"Razgovor može imati najviše {AssistantChatCommand.MaxMessages} poruka.")
            .Must(m => m.Count > 0 && m[^1].Role == AiRoles.User)
            .WithMessage("Zadnja poruka mora biti pitanje korisnika.");

        RuleForEach(x => x.Messages).ChildRules(m =>
        {
            // Only user/assistant – a client must never inject "system" or "tool" messages
            m.RuleFor(x => x.Role)
                .Must(r => r is AiRoles.User or AiRoles.Assistant)
                .WithMessage("Neispravna uloga poruke.");

            m.RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Poruka ne može biti prazna.")
                .MaximumLength(AssistantChatCommand.MaxMessageLength)
                .WithMessage($"Poruka može imati najviše {AssistantChatCommand.MaxMessageLength} znakova.");
        });
    }
}

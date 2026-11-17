namespace MediCare.Application.Modules.Assistant.Commands.Chat;

public sealed class AssistantChatResponseDto
{
    /// <summary>The assistant's answer (simple markdown: **bold**, "- " lists).</summary>
    public string Reply { get; init; } = string.Empty;

    /// <summary>Items the assistant recommended – the UI shows them as buttons. Already checked against the database.</summary>
    public AssistantSuggestionsDto Suggestions { get; init; } = new();

    /// <summary>Admin only: tables with personal data (patient names, notes) shown by the app, not seen by the AI service.</summary>
    public List<AssistantTableDto> Details { get; init; } = [];
}

public sealed class AssistantSuggestionsDto
{
    public List<AssistantMedicineSuggestionDto> Medicines { get; init; } = [];
    public List<AssistantTreatmentSuggestionDto> Treatments { get; init; } = [];
    public List<AssistantSlotSuggestionDto> Slots { get; init; } = [];
}

public sealed class AssistantMedicineSuggestionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool InStock { get; init; }
}

public sealed class AssistantTreatmentSuggestionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

/// <summary>A concrete free appointment: the UI opens the booking calendar with it preselected.</summary>
public sealed class AssistantSlotSuggestionDto
{
    public int TreatmentId { get; init; }
    public string TreatmentName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    /// <summary>yyyy-MM-dd</summary>
    public string Date { get; init; } = string.Empty;
    /// <summary>HH:mm</summary>
    public string Time { get; init; } = string.Empty;
}

/// <summary>
/// A table that the APP attaches to the answer for the admin (names, notes, customers).
/// It is built from the database after the tool ran and goes straight to the browser:
/// it is never put into the messages that are sent to the external AI service.
/// </summary>
public sealed class AssistantTableDto
{
    public string Title { get; init; } = string.Empty;
    public List<string> Columns { get; init; } = [];
    public List<List<string>> Rows { get; init; } = [];
}

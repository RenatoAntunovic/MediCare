namespace MediCare.Shared.Options;

/// <summary>
/// Groq (OpenAI-compatible) settings from the "Groq" section.
/// The API key is a secret – set it with user-secrets, never in appsettings.json:
///   dotnet user-secrets set "Groq:ApiKey" "gsk_..."
/// </summary>
public sealed class GroqOptions
{
    public const string SectionName = "Groq";

    public string? ApiKey { get; init; }
    public string BaseUrl { get; init; } = "https://api.groq.com/openai/v1/";
    public string Model { get; init; } = "openai/gpt-oss-120b";
}

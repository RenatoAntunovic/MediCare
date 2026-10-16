using System.Globalization;
using System.Text;
using System.Text.Json;

namespace MediCare.Application.Modules.Assistant.Tools;

/// <summary>
/// A set of functions the model may call for one kind of user (patient or admin).
/// Tool results are plain objects; the handler serializes them to JSON for the model.
/// </summary>
public interface IAssistantToolbox
{
    IReadOnlyList<AiTool> Tools { get; }

    /// <summary>Unknown tool or bad arguments → return an { error } object, never throw (the model can recover).</summary>
    Task<object> ExecuteAsync(string name, JsonElement args, CancellationToken ct);
}

/// <summary>Reading tool arguments safely – the model sometimes sends numbers as strings or omits fields.</summary>
internal static class ToolArgs
{
    public static string? String(JsonElement args, string name) =>
        args.ValueKind == JsonValueKind.Object &&
        args.TryGetProperty(name, out var v) &&
        v.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(v.GetString())
            ? v.GetString()!.Trim()
            : null;

    public static int? Int(JsonElement args, string name)
    {
        if (args.ValueKind != JsonValueKind.Object || !args.TryGetProperty(name, out var v))
            return null;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n))
            return n;
        if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out n))
            return n;
        return null;
    }

    public static bool? Bool(JsonElement args, string name) =>
        args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var v) &&
        v.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? v.GetBoolean()
            : null;

    /// <summary>yyyy-MM-dd</summary>
    public static DateTime? Date(JsonElement args, string name) =>
        DateTime.TryParseExact(String(args, name), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d.Date
            : null;

    public static IEnumerable<JsonElement> Array(JsonElement args, string name) =>
        args.ValueKind == JsonValueKind.Object &&
        args.TryGetProperty(name, out var v) &&
        v.ValueKind == JsonValueKind.Array
            ? v.EnumerateArray().ToList()
            : [];
}

internal static class ToolText
{
    /// <summary>"Glavobolja" and "glavobolja", "čaj" and "caj" match the same – for searching the catalog.</summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var decomposed = value.ToLowerInvariant().Replace("đ", "dj").Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString();
    }

    public static string Day(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public static string Time(TimeSpan t) => t.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
}

/// <summary>JSON Schemas for tool parameters (kept small – every token counts on the free plan).</summary>
internal static class ToolSchemas
{
    public const string None = """{"type":"object","properties":{}}""";

    public const string DateRange = """
        {"type":"object","properties":{
          "from":{"type":"string","description":"Start date yyyy-MM-dd (inclusive)"},
          "to":{"type":"string","description":"End date yyyy-MM-dd (inclusive)"}}}
        """;
}

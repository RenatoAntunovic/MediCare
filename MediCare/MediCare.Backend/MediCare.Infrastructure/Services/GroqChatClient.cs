using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using MediCare.Application.Abstractions;
using MediCare.Application.Common.Exceptions;
using MediCare.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediCare.Infrastructure.Services;

/// <summary>
/// Groq chat completions (OpenAI-compatible): POST {BaseUrl}chat/completions.
/// The API key never leaves the backend.
/// </summary>
public sealed class GroqChatClient(HttpClient http, IOptions<GroqOptions> options, ILogger<GroqChatClient> logger)
    : IAiChatClient
{
    private readonly GroqOptions _options = options.Value;

    public async Task<AiChatResult> CompleteAsync(
        IReadOnlyList<AiMessage> messages,
        IReadOnlyList<AiTool> tools,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new MediCareServiceUnavailableException(
                "AI asistent nije podešen (nedostaje Groq API ključ).");

        var body = new JsonObject
        {
            ["model"] = _options.Model,
            ["messages"] = new JsonArray(messages.Select(ToJson).ToArray()),
            ["temperature"] = 0.3,
            ["max_completion_tokens"] = 1024,
        };

        if (tools.Count > 0)
        {
            body["tools"] = new JsonArray(tools.Select(t => (JsonNode)new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = t.Name,
                    ["description"] = t.Description,
                    ["parameters"] = JsonNode.Parse(t.ParametersJsonSchema)
                }
            }).ToArray());
            body["tool_choice"] = "auto";
        }

        // gpt-oss models "think" before answering – low effort keeps replies fast
        if (_options.Model.StartsWith("openai/gpt-oss", StringComparison.OrdinalIgnoreCase))
            body["reasoning_effort"] = "low";

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_options.BaseUrl), "chat/completions"))
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException) && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Groq request failed.");
            throw new MediCareServiceUnavailableException("AI asistent trenutno nije dostupan. Pokušajte ponovo.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning("Groq returned {Status}: {Error}", (int)response.StatusCode, error);

                throw new MediCareServiceUnavailableException(response.StatusCode switch
                {
                    HttpStatusCode.TooManyRequests => "AI asistent je trenutno preopterećen (limit besplatnog plana). Pokušajte za minutu.",
                    HttpStatusCode.Unauthorized => "AI asistent nije ispravno podešen (neispravan Groq API ključ).",
                    _ => "AI asistent trenutno nije dostupan. Pokušajte ponovo."
                });
            }

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var message = json.RootElement.GetProperty("choices")[0].GetProperty("message");

            var content = message.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String
                ? c.GetString()
                : null;

            var toolCalls = new List<AiToolCall>();
            if (message.TryGetProperty("tool_calls", out var calls) && calls.ValueKind == JsonValueKind.Array)
            {
                foreach (var call in calls.EnumerateArray())
                {
                    var function = call.GetProperty("function");
                    toolCalls.Add(new AiToolCall(
                        call.GetProperty("id").GetString()!,
                        function.GetProperty("name").GetString()!,
                        function.TryGetProperty("arguments", out var args) ? args.GetString() ?? "{}" : "{}"));
                }
            }

            return new AiChatResult(content, toolCalls);
        }
    }

    private static JsonNode ToJson(AiMessage m)
    {
        var node = new JsonObject { ["role"] = m.Role, ["content"] = m.Content };

        if (m.ToolCalls is { Count: > 0 })
        {
            node["tool_calls"] = new JsonArray(m.ToolCalls.Select(tc => (JsonNode)new JsonObject
            {
                ["id"] = tc.Id,
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = tc.Name, ["arguments"] = tc.ArgumentsJson }
            }).ToArray());
        }

        if (m.ToolCallId is not null)
            node["tool_call_id"] = m.ToolCallId;

        return node;
    }
}

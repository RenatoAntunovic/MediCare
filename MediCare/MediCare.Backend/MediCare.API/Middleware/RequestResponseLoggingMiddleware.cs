using System.Diagnostics;
using System.Text;

namespace Market.API.Middlewares;

/// <summary>
/// Logs incoming HTTP requests: method, path, status code and duration.
/// Request bodies are logged only for JSON requests, never for /api/auth (passwords, tokens).
/// Response bodies are never logged (they can contain tokens or large files).
/// </summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int SlowRequestThresholdMs = 400;
    private const int MaxLoggedBodyLength = 2000;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        string? requestBody = null;
        if (ShouldLogRequestBody(request))
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (requestBody.Length > MaxLoggedBodyLength)
                requestBody = requestBody[..MaxLoggedBodyLength] + "... (truncated)";
        }

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;

            var logMessage = new StringBuilder()
                .AppendLine("HTTP Request Log:")
                .AppendLine($"  Path: {request.Path}")
                .AppendLine($"  Method: {request.Method}")
                .AppendLine($"  Status: {context.Response.StatusCode}")
                .AppendLine($"  Duration: {elapsed} ms");

            if (!string.IsNullOrWhiteSpace(requestBody))
                logMessage.AppendLine($"  Request Body: {requestBody}");

            logger.LogInformation("{Log}", logMessage.ToString());

            if (elapsed > SlowRequestThresholdMs)
            {
                logger.LogWarning("[SLOW REQUEST] {Path} took {Elapsed} ms", request.Path, elapsed);
                Directory.CreateDirectory("Logs");
                await File.AppendAllTextAsync("Logs/slow-requests.log",
                    $"{DateTime.UtcNow:u} | {request.Path} | {elapsed} ms{Environment.NewLine}");
            }
        }
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        if (request.Method is not ("POST" or "PUT"))
            return false;

        // Never log auth bodies: they contain passwords and refresh tokens
        if (request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
            return false;

        // Only JSON (skip file uploads / multipart forms)
        return request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
    }
}
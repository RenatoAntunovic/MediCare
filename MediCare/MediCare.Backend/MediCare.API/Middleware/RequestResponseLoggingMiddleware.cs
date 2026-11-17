using System.Diagnostics;
using System.Text;

namespace Market.API.Middlewares;

/// <summary>
/// Logs incoming HTTP requests: method, path, status code and duration – nothing else.
/// Request and response bodies are NEVER logged: they can contain passwords (change password),
/// chat messages with the AI assistant, medical notes (reservations) and tokens.
/// A list of "safe" paths would be easy to get wrong, so there is no list – no body is read at all.
/// The path is logged without the query string (request.Path never includes it).
/// </summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int SlowRequestThresholdMs = 400;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

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
}
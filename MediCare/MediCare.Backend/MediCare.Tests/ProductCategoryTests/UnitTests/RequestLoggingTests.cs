using System.Text;
using Market.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MediCare.Tests.UnitTests;

/// <summary>The request log keeps method, path, status and duration – never any body text.</summary>
public class RequestLoggingTests
{
    /// <summary>Fake logger: keeps the formatted text of every log entry.</summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Entries { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add(formatter(state, exception));
    }

    private static async Task<string> SendAsync(string method, string path, string query, string body)
    {
        var logger = new CapturingLogger<RequestResponseLoggingMiddleware>();
        // the next middleware reads the body like a controller would, and answers 204
        var middleware = new RequestResponseLoggingMiddleware(async ctx =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            Assert.Equal(body, await reader.ReadToEndAsync()); // the request is still intact for the controller
            ctx.Response.StatusCode = 204;
        }, logger);

        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

        await middleware.InvokeAsync(context);
        return string.Join("\n", logger.Entries);
    }

    [Theory]
    [InlineData("POST", "/api/Profile/change-password", """{"currentPassword":"S3cretOld!","newPassword":"Sup3rSecret!","confirmPassword":"Sup3rSecret!"}""", "S3cretOld!", "Sup3rSecret!")]
    [InlineData("POST", "/api/Assistant/chat", """{"messages":[{"role":"user","content":"Imam dijabetes i boli me glava"}]}""", "dijabetes", "boli me glava")]
    [InlineData("PUT", "/api/Reservations/5", """{"notes":"alergija na penicilin","reservationTime":"10:30"}""", "alergija", "penicilin")]
    [InlineData("POST", "/api/Reservations", """{"treatmentId":1,"notes":"trudnoca, 12. sedmica"}""", "trudnoca", "sedmica")]
    [InlineData("POST", "/api/auth/login", """{"email":"a@a.com","password":"Lozinka123!"}""", "Lozinka123!", "a@a.com")]
    public async Task Log_HasMethodPathStatusDuration_AndNoBodyText(string method, string path, string body, string secret1, string secret2)
    {
        var log = await SendAsync(method, path, "?token=query-secret-123", body);

        Assert.Contains(method, log);
        Assert.Contains(path, log);
        Assert.Contains("204", log);
        Assert.Contains("Duration", log);
        Assert.DoesNotContain("Body", log);
        Assert.DoesNotContain(secret1, log, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(secret2, log, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("query-secret-123", log); // the query string is not logged either
    }
}

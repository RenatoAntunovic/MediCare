namespace MediCare.Application.Common.Exceptions;

/// <summary>
/// An external service the request depends on (e.g. the AI provider) is not configured or not responding.
/// Mapped to 503 – the message is safe to show to the user.
/// </summary>
public sealed class MediCareServiceUnavailableException : Exception
{
    public MediCareServiceUnavailableException(string message) : base(message) { }

    public MediCareServiceUnavailableException(string message, Exception? innerException)
        : base(message, innerException) { }
}

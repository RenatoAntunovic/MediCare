namespace MediCare.Application.Common.Exceptions;

public sealed class MarketConflictException : Exception
{
    public MarketConflictException(string message) : base(message) { }
}

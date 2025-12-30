namespace MediCare.Application.Common.Exceptions;

public sealed class MediCareConflictException : Exception
{
    public MediCareConflictException(string message) : base(message) { }
}

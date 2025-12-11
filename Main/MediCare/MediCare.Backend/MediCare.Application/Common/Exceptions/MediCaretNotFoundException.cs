namespace MediCare.Application.Common.Exceptions;

public sealed class MediCareNotFoundException : Exception
{
    public MediCareNotFoundException(string message) : base(message) { }
}

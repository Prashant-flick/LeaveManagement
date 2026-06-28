namespace Leave.Application.Common.Exceptions;

public abstract class BaseException : Exception
{
    public int StatusCode { get; }

    protected BaseException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
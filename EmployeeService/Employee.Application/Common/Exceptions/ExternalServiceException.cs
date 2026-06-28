namespace Employee.Application.Common.Exceptions;
public class ExternalServiceException : BaseException
{
    public ExternalServiceException(string message)
        : base(message, 503)
    {
    }
}
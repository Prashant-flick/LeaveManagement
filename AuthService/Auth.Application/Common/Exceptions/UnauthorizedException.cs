namespace Auth.Application.Common.Exceptions;
public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message)
        : base(message, 403)
    {
    }
}
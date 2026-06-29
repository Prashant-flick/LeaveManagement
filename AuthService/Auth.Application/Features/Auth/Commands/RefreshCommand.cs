using MediatR;

namespace Auth.Application.Features.Auth.Commands
{
    public record RefreshCommand(string RefreshToken) : IRequest<LoginResponse>;
}

using MediatR;

namespace Auth.Application.Features.Auth.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
}

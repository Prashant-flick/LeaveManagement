using MediatR;

namespace Auth.Application.Features.Auth.Commands
{
    public record RegisterCommand(string Email, string Password) : IRequest<RegisterResponse>;
}

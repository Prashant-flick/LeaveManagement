using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.Register
{
    public record RegisterCommand(string Email, string Password) : IRequest<RegisterResponse>;
}

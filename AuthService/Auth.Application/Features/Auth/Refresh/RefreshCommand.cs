using MediatR;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.Refresh
{
    public record RefreshCommand(string RefreshToken) : IRequest<LoginResponse>;
}

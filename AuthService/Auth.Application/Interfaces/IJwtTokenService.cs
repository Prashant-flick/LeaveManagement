using System.Security.Claims;
using Auth.Domain.Entities;

namespace Auth.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, List<string> roles, int employeeId);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
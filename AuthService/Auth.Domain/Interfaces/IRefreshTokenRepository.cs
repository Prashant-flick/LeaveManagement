using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;
public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
}
using MediatR;
using Auth.Application.Common.Exceptions;
using Auth.Application.Interfaces;
using Auth.Domain.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands
{
    public class RefreshCommandHandler : IRequestHandler<RefreshCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RefreshCommandHandler> _logger;
        private readonly IJwtTokenService _jwtService;
        private readonly IEmployeeClient _employeeClient;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshCommandHandler(
            IUserRepository userRepository, 
            ILogger<RefreshCommandHandler> _logger, 
            IJwtTokenService _jwtService,
            IEmployeeClient _employeeClient,
            IRefreshTokenRepository _refreshTokenRepository,
            IUnitOfWork _unitOfWork)
        {
            this._userRepository = userRepository;
            this._logger = _logger;
            this._jwtService = _jwtService;
            this._employeeClient = _employeeClient;
            this._refreshTokenRepository = _refreshTokenRepository;
            this._unitOfWork = _unitOfWork;
        }

        public async Task<LoginResponse> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refresh token attempt.");

            var savedRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (savedRefreshToken == null)
            {
                _logger.LogWarning("Refresh token not found in database");
                throw new UnauthorizedException("Invalid refresh token");
            }

            if (savedRefreshToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token is revoked: {RefreshTokenId}", savedRefreshToken.Id);
                throw new UnauthorizedException("Refresh token is revoked");
            }

            if (savedRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token has expired at {ExpiresAt}", savedRefreshToken.ExpiresAt);
                throw new UnauthorizedException("Refresh token has expired");
            }

            var userId = savedRefreshToken.UserId;
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                throw new NotFoundException("User not found");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("User is inactive: {UserId}", userId);
                throw new UnauthorizedException("User is inactive");
            }

            savedRefreshToken.IsRevoked = true;

            var userRoleResponse = await _employeeClient.GetRolesAndEmployeeIdByUserIdAsync(user.Id);
            var newAccessToken = _jwtService.GenerateToken(user, userRoleResponse.Roles, userRoleResponse.EmployeeId);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddRefreshTokenAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully refreshed token for UserId: {UserId}", userId);

            return new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                EmployeeId = userRoleResponse.EmployeeId,
                Roles = userRoleResponse.Roles,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}

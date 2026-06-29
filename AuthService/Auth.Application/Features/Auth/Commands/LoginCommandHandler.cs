using MediatR;
using Auth.Application.Common.Exceptions;
using Auth.Application.Interfaces;
using Auth.Domain.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IJwtTokenService _jwtService;
        private readonly IEmployeeClient _employeeClient;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(
            IUserRepository userRepository, 
            ILogger<LoginCommandHandler> logger, 
            IJwtTokenService jwtService,
            IEmployeeClient employeeClient,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _logger = logger;
            _jwtService = jwtService;
            _employeeClient = employeeClient;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login attempt for Email: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed - User not found for Email: {Email}", request.Email);
                throw new NotFoundException("User with Email not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed - Invalid password for Email: {Email}", request.Email);
                throw new UnauthorizedException("Invalid credentials");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed - Inactive user: {UserId}", user.Id);
                throw new UnauthorizedException("User is inactive");
            }

            _logger.LogInformation(
                "Login successful for UserId: {UserId}, Email: {Email}",
                user.Id,
                user.Email
            );

            var userRoleResponse = await _employeeClient.GetRolesAndEmployeeIdByUserIdAsync(user.Id);

            var accessToken = _jwtService.GenerateToken(user, userRoleResponse.Roles, userRoleResponse.EmployeeId);
            
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddRefreshTokenAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                EmployeeId = userRoleResponse.EmployeeId,
                Roles = userRoleResponse.Roles,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}

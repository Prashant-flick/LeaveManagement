using Auth.Application.Common.Exceptions;
using Auth.Application.Interfaces;
using Auth.Domain.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly IJwtTokenService _jwtService;
        private readonly IEmployeeClient _employeeClient;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepository, 
            ILogger<AuthService> logger, 
            IJwtTokenService jwtService,
            IEmployeeClient employeeClient,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork
        ){
            _userRepository = userRepository;
            _logger = logger;
            _jwtService = jwtService;
            _employeeClient = employeeClient;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Register attempt for Email: {Email}", request.Email);

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Register failed - User already exists for Email: {Email}", request.Email);
                throw new BadRequestException("User already exists");
            }

            var user = new User
            {
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User registered successfully with Id: {UserId}, Email: {Email}", user.Id, user.Email);

            return new RegisterResponse
            {
                Id = user.Id,
                Email = user.Email
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
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
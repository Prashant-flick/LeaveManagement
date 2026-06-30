using MediatR;
using Auth.Application.Common.Exceptions;
using Auth.Domain.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            ILogger<RegisterCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered successfully with Id: {UserId}, Email: {Email}", user.Id, user.Email);

            return new RegisterResponse
            {
                Id = user.Id,
                Email = user.Email
            };
        }
    }
}

using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            ApplicationDbContext context,
            ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation(
                "Fetching user by Email: {Email}",
                email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning(
                    "User not found for Email: {Email}",
                    email);
            }

            return user;
        }

        public async Task AddAsync(User user)
        {
            _logger.LogInformation(
                "Adding new user with Email: {Email}",
                user.Email);

            await _context.Users.AddAsync(user);
        }
    }
}
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Data
{
    public static class AuthDataSeeder
    {
        public static async Task SeedAdminUserAsync(ApplicationDbContext context, ILogger logger)
        {
            if (await context.Users.AnyAsync(u => u.Email == "admin@example.com"))
            {
                logger.LogInformation("Admin user already exists. Skipping seeding.");
                return;
            }

            logger.LogInformation("Seeding default admin user...");

            var adminUser = new User
            {
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            logger.LogInformation("Default admin user seeded successfully.");
        }
    }
}

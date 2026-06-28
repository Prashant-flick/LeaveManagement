using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Employee.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedRolesAsync(
        ApplicationDbContext context,
        ILogger logger)
    {
        if (await context.Roles.AnyAsync(r =>
            r.Name.ToLower() == "admin"))
        {
            logger.LogInformation("Roles already exist. Skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding default roles...");

        var roles = new List<Role>
        {
            new Role { Name = "Admin" },
            new Role { Name = "Manager" },
            new Role { Name = "Employee" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        logger.LogInformation("Default roles seeded successfully.");
    }
}
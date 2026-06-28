using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Employee.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedRolesAndAdminAsync(
        ApplicationDbContext context,
        ILogger logger)
    {
        // 1. Seed Roles
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "admin");
        var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "manager");
        var employeeRole = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "employee");

        if (adminRole == null)
        {
            logger.LogInformation("Seeding default roles...");
            adminRole = new Role { Name = "Admin" };
            managerRole = new Role { Name = "Manager" };
            employeeRole = new Role { Name = "Employee" };
            
            await context.Roles.AddRangeAsync(new[] { adminRole, managerRole, employeeRole });
            await context.SaveChangesAsync();
        }

        // 2. Seed Admin Employee (mapped to UserId = 1)
        var adminUserId = 1; 
        var adminEmployee = await context.Employees.FirstOrDefaultAsync(e => e.UserId == adminUserId);
        if (adminEmployee == null)
        {
            logger.LogInformation("Seeding default admin employee (UserId: 1)...");
            adminEmployee = new Employee.Domain.Entities.Employee
            {
                UserId = adminUserId,
                FirstName = "System",
                LastName = "Admin",
                Department = "IT",
                IsActive = true
            };

            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync(); // Generates ID for the employee

            // Link Employee to Role
            var employeeRoleMapping = new EmployeeRole
            {
                EmployeeId = adminEmployee.Id,
                RoleId = adminRole.Id
            };

            await context.EmployeeRoles.AddAsync(employeeRoleMapping);
            await context.SaveChangesAsync();
            logger.LogInformation("Default admin employee and role mapping seeded successfully.");
        }
    }
}
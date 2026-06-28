using Employee.Domain.Entities;
using Employee.Domain.Interfaces;
using Employee.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Employee.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RoleRepository> _logger;

    public RoleRepository(ApplicationDbContext context, ILogger<RoleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(Role role)
    {
        _logger.LogInformation("Adding role: {RoleName}", role.Name);

        await _context.Roles.AddAsync(role);
    }

    public async Task<List<Role>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all roles");

        return await _context.Roles.ToListAsync();
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        _logger.LogInformation("Fetching role by name: {RoleName}", name);

        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }
}
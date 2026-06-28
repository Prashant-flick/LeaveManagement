using Employee.Domain.Entities;
using Employee.Domain.Interfaces;
using Employee.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Employee.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeRepository> _logger;

    public EmployeeRepository(
        ApplicationDbContext context,
        ILogger<EmployeeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Domain.Entities.Employee?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching employee by Id: {EmployeeId}", id);

        var result = await _context.Employees
            .Include(e => e.EmployeeRoles!)
            .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (result == null)
        {
            _logger.LogWarning("Employee not found for Id: {EmployeeId}", id);
        }

        return result;
    }

    public async Task<Domain.Entities.Employee?> GetByUserIdAsync(int userId)
    {
        _logger.LogInformation("Fetching employee by UserId: {UserId}", userId);

        var result = await _context.Employees
            .Include(e => e.EmployeeRoles!)
            .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (result == null)
        {
            _logger.LogWarning("Employee not found for UserId: {UserId}", userId);
        }

        return result;
    }

    public async Task<List<Domain.Entities.Employee>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all employees");

        var result = await _context.Employees
            .Include(e => e.EmployeeRoles!)
            .ThenInclude(er => er.Role)
            .ToListAsync();

        _logger.LogInformation("Total employees fetched: {Count}", result.Count);

        return result;
    }

    public async Task AddAsync(Domain.Entities.Employee employee)
    {
        _logger.LogInformation(
            "Adding employee for UserId: {UserId}",
            employee.UserId);

        await _context.Employees.AddAsync(employee);
    }

    public void RemoveEmployeeRoles(IEnumerable<EmployeeRole> roles)
    {
        var count = roles?.Count() ?? 0;

        _logger.LogInformation(
            "Removing {Count} employee roles",
            count);

        _context.EmployeeRoles.RemoveRange(roles);
    }

    public async Task AddEmployeeRolesAsync(IEnumerable<EmployeeRole> roles)
    {
        var count = roles?.Count() ?? 0;

        _logger.LogInformation(
            "Adding {Count} employee roles",
            count);

        await _context.EmployeeRoles.AddRangeAsync(roles);
    }
}
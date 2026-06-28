using Employee.Application.Common.Exceptions;
using Employee.Application.Interfaces;
using Employee.Domain.Common.Interfaces;
using Employee.Domain.Entities;
using Employee.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Employee.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;
    private readonly ILogger<RoleService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(
        IRoleRepository repository,
        ILogger<RoleService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Role> CreateRoleAsync(string name)
    {
        _logger.LogInformation("Creating role: {RoleName}", name);

        // ✅ Validate input (important)
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Invalid role name received");

            throw new BadRequestException("Role name is required");
        }

        var existing = await _repository.GetByNameAsync(name);

        if (existing != null)
        {
            _logger.LogWarning(
                "Role already exists: {RoleName}",
                name);

            throw new BadRequestException("Role already exists");
        }

        var role = new Role
        {
            Name = name.Trim()
        };

        await _repository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Role created successfully: {RoleName}",
            role.Name);

        return role;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        _logger.LogInformation("Fetching all roles");

        var roles = await _repository.GetAllAsync();

        if (roles == null || !roles.Any())
        {
            _logger.LogWarning("No roles found");

            return new List<Role>();
        }

        return roles;
    }
}
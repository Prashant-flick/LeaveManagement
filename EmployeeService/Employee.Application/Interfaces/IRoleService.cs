using Employee.Domain.Entities;

namespace Employee.Application.Interfaces;

public interface IRoleService
{
    Task<Role> CreateRoleAsync(string name);
    Task<List<Role>> GetAllRolesAsync();
}
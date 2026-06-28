using Employee.Domain.Entities;

namespace Employee.Domain.Interfaces;

public interface IRoleRepository
{
    Task AddAsync(Role role);
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByNameAsync(string name);
}
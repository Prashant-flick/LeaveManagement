using Employee.Domain.Entities;

namespace Employee.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<Entities.Employee?> GetByIdAsync(int id);
    Task<Entities.Employee?> GetByUserIdAsync(int userId);

    Task<List<Entities.Employee>> GetAllAsync();

    Task AddAsync(Entities.Employee employee);

    void RemoveEmployeeRoles(IEnumerable<EmployeeRole> roles);
    Task AddEmployeeRolesAsync(IEnumerable<EmployeeRole> roles);
}
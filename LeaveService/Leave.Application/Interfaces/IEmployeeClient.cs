namespace Leave.Application.Interfaces;

public interface IEmployeeClient
{
    Task<int?> GetManagerIdAsync(int employeeId);

    Task<bool> EmployeeExistsAsync(int employeeId);
}
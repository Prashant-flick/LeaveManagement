using Employee.Application.DTOs;

namespace Employee.Application.Interfaces{
    public interface IEmployeeService
    {
        Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<List<EmployeeResponse>> GetEmployeesAsync();
        Task<EmployeeResponse?> GetEmployeeByIdAsync(int id);
        Task<EmployeeResponse?> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request);
        Task<bool> DeleteEmployeeAsync(int id);

        Task<UserRoleResponse> GetRolesAndEmployeeIdByUserId(int userId);
    }
}
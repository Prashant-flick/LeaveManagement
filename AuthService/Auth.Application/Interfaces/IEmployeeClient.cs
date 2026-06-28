using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

public interface IEmployeeClient
{
    Task<UserRoleResponse> GetRolesAndEmployeeIdByUserIdAsync(int userId);
}
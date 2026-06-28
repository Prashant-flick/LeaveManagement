namespace Employee.Application.DTOs;

public class UserRoleResponse
{
    public int EmployeeId { get; set; }

    public List<string> Roles { get; set; } = new();
}
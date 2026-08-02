using MediatR;
using Employee.Application.DTOs;
using System.Collections.Generic;

namespace Employee.Application.Features.Employees.Commands.UpdateEmployee
{
    public record UpdateEmployeeCommand(
        int Id,
        string? FirstName,
        string? LastName,
        string? Department,
        bool? IsActive,
        int? ManagerId,
        List<int>? RoleIds,
        int CurrentUserId,
        int CurrentEmployeeId,
        List<string> CurrentUserRoles
    ) : IRequest<EmployeeResponse>;
}

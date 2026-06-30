using MediatR;
using Employee.Application.DTOs;
using System.Collections.Generic;

namespace Employee.Application.Features.Employees.Commands.CreateEmployee
{
    public record CreateEmployeeCommand(
        int UserId,
        string FirstName,
        string LastName,
        string Department,
        int? ManagerId,
        List<int> RoleIds
    ) : IRequest<EmployeeResponse>;
}

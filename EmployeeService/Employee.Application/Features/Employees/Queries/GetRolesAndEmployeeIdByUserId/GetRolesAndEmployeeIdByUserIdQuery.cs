using MediatR;
using Employee.Application.DTOs;

namespace Employee.Application.Features.Employees.Queries.GetRolesAndEmployeeIdByUserId
{
    public record GetRolesAndEmployeeIdByUserIdQuery(int UserId) : IRequest<UserRoleResponse>;
}

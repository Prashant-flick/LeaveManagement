using MediatR;
using Employee.Application.DTOs;

namespace Employee.Application.Features.Employees.Queries.GetEmployeeById
{
    public record GetEmployeeByIdQuery(int Id) : IRequest<EmployeeResponse>;
}

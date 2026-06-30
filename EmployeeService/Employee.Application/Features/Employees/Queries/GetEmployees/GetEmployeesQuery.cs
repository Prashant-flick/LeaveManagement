using MediatR;
using Employee.Application.DTOs;
using System.Collections.Generic;

namespace Employee.Application.Features.Employees.Queries.GetEmployees
{
    public record GetEmployeesQuery() : IRequest<List<EmployeeResponse>>;
}

using MediatR;

namespace Employee.Application.Features.Employees.Commands.DeleteEmployee
{
    public record DeleteEmployeeCommand(int Id) : IRequest<bool>;
}

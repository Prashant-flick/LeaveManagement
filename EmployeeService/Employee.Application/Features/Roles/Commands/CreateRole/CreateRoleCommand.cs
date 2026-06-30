using MediatR;
using Employee.Domain.Entities;

namespace Employee.Application.Features.Roles.Commands.CreateRole
{
    public record CreateRoleCommand(string Name) : IRequest<Role>;
}

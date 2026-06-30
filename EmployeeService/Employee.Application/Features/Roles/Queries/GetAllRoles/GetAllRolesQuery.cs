using MediatR;
using Employee.Domain.Entities;
using System.Collections.Generic;

namespace Employee.Application.Features.Roles.Queries.GetAllRoles
{
    public record GetAllRolesQuery() : IRequest<List<Role>>;
}

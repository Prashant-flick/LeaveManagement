using MediatR;
using Employee.Domain.Entities;
using Employee.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Employee.Application.Features.Roles.Queries.GetAllRoles
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<Role>>
    {
        private readonly IRoleRepository _repository;
        private readonly ILogger<GetAllRolesQueryHandler> _logger;

        public GetAllRolesQueryHandler(
            IRoleRepository repository,
            ILogger<GetAllRolesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<Role>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _repository.GetAllAsync();

            if (roles == null || !roles.Any())
            {
                _logger.LogWarning("No roles found");
                return new List<Role>();
            }

            return roles;
        }
    }
}

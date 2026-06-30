using MediatR;
using Employee.Application.DTOs;
using Employee.Application.Common.Exceptions;
using Employee.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Employee.Application.Features.Employees.Queries.GetRolesAndEmployeeIdByUserId
{
    public class GetRolesAndEmployeeIdByUserIdQueryHandler : IRequestHandler<GetRolesAndEmployeeIdByUserIdQuery, UserRoleResponse>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<GetRolesAndEmployeeIdByUserIdQueryHandler> _logger;

        public GetRolesAndEmployeeIdByUserIdQueryHandler(
            IEmployeeRepository repository,
            ILogger<GetRolesAndEmployeeIdByUserIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<UserRoleResponse> Handle(GetRolesAndEmployeeIdByUserIdQuery request, CancellationToken cancellationToken)
        {
            var employee = await _repository.GetByUserIdAsync(request.UserId);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found for UserId: {UserId}", request.UserId);
                throw new NotFoundException("Employee not found for given user");
            }

            var roles = employee.EmployeeRoles!
                .Select(r => r.Role.Name)
                .ToList();

            _logger.LogInformation("Roles fetched for UserId: {UserId}: {Roles}", request.UserId, string.Join(",", roles));

            return new UserRoleResponse
            {
                EmployeeId = employee.Id,
                Roles = roles
            };
        }
    }
}

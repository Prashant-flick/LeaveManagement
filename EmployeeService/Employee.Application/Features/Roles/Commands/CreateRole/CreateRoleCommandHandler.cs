using MediatR;
using Employee.Application.Common.Exceptions;
using Employee.Domain.Common.Interfaces;
using Employee.Domain.Entities;
using Employee.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Employee.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Role>
    {
        private readonly IRoleRepository _repository;
        private readonly ILogger<CreateRoleCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoleCommandHandler(
            IRoleRepository repository,
            ILogger<CreateRoleCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Role> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByNameAsync(request.Name);

            if (existing != null)
            {
                _logger.LogWarning("Role already exists: {RoleName}", request.Name);
                throw new BadRequestException("Role already exists");
            }

            var role = new Role
            {
                Name = request.Name.Trim()
            };

            await _repository.AddAsync(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role created successfully: {RoleName}", role.Name);

            return role;
        }
    }
}

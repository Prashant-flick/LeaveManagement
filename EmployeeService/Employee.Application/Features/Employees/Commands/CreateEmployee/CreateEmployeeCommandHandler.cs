using MediatR;
using Employee.Application.DTOs;
using Employee.Application.Common.Exceptions;
using Employee.Domain.Common.Interfaces;
using Employee.Domain.Entities;
using Employee.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Employee.Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeResponse>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<CreateEmployeeCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CreateEmployeeCommandHandler(
            IEmployeeRepository repository,
            ILogger<CreateEmployeeCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<EmployeeResponse> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByUserIdAsync(request.UserId);

            if (existing != null)
            {
                _logger.LogWarning("Employee already exists for UserId: {UserId}", request.UserId);
                throw new BadRequestException("Employee already exists for this user");
            }

            var employee = new Domain.Entities.Employee
            {
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                ManagerId = request.ManagerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.AddAsync(employee);

            if (request.RoleIds != null && request.RoleIds.Any())
            {
                var roles = request.RoleIds.Select(roleId => new EmployeeRole
                {
                    EmployeeId = employee.Id,
                    RoleId = roleId
                });

                await _repository.AddEmployeeRolesAsync(roles);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee created successfully with Id: {EmployeeId}", employee.Id);

            // Fetch the created employee with roles to return a complete response
            var createdEmployee = await _repository.GetByIdAsync(employee.Id);
            if (createdEmployee == null)
            {
                throw new NotFoundException("Employee not found after creation");
            }

            return new EmployeeResponse
            {
                Id = createdEmployee.Id,
                UserId = createdEmployee.UserId,
                FirstName = createdEmployee.FirstName,
                LastName = createdEmployee.LastName,
                Department = createdEmployee.Department,
                IsActive = createdEmployee.IsActive,
                ManagerId = createdEmployee.ManagerId,
                Roles = createdEmployee.EmployeeRoles?
                    .Select(r => r.Role?.Name ?? "")
                    .ToList() ?? new List<string>()
            };
        }
    }
}

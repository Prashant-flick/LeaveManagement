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

namespace Employee.Application.Features.Employees.Commands.UpdateEmployee
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeResponse>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<UpdateEmployeeCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateEmployeeCommandHandler(
            IEmployeeRepository repository,
            ILogger<UpdateEmployeeCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<EmployeeResponse> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _repository.GetByIdAsync(request.Id);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found for update: {EmployeeId}", request.Id);
                throw new NotFoundException("Employee not found");
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                employee.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                employee.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.Department))
                employee.Department = request.Department;

            if (request.IsActive.HasValue)
                employee.IsActive = request.IsActive.Value;

            if (request.ManagerId.HasValue)
                employee.ManagerId = request.ManagerId;

            employee.UpdatedAt = DateTime.UtcNow;

            if (request.RoleIds != null && request.RoleIds.Any())
            {
                _logger.LogInformation("Updating roles for EmployeeId: {EmployeeId}", request.Id);

                _repository.RemoveEmployeeRoles(employee.EmployeeRoles!);

                var newRoles = request.RoleIds.Select(roleId => new EmployeeRole
                {
                    EmployeeId = employee.Id,
                    RoleId = roleId
                });

                await _repository.AddEmployeeRolesAsync(newRoles);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Employee updated successfully: {EmployeeId}", request.Id);

            var updatedEmployee = await _repository.GetByIdAsync(request.Id);
            if (updatedEmployee == null)
            {
                throw new NotFoundException("Employee not found after update");
            }

            return new EmployeeResponse
            {
                Id = updatedEmployee.Id,
                UserId = updatedEmployee.UserId,
                FirstName = updatedEmployee.FirstName,
                LastName = updatedEmployee.LastName,
                Department = updatedEmployee.Department,
                IsActive = updatedEmployee.IsActive,
                ManagerId = updatedEmployee.ManagerId,
                Roles = updatedEmployee.EmployeeRoles?
                    .Select(r => r.Role?.Name ?? "")
                    .ToList() ?? new List<string>()
            };
        }
    }
}

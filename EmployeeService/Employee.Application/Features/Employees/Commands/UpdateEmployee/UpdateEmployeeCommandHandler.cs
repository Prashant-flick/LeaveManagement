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

            var isAdmin = request.CurrentUserRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
            var isManager = request.CurrentUserRoles.Contains("Manager", StringComparer.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                // Check if editing their own profile
                var isSelfUpdate = employee.UserId == request.CurrentUserId;

                // Check if editing a direct report (must be a Manager and the report's ManagerId must match the caller's EmployeeId)
                var isDirectReport = isManager && employee.ManagerId == request.CurrentEmployeeId;

                if (!isSelfUpdate && !isDirectReport)
                {
                    _logger.LogWarning(
                        "Unauthorized update attempt: Caller UserId {CallerUserId} (EmployeeId {CallerEmployeeId}) tried to edit EmployeeId {TargetEmployeeId}",
                        request.CurrentUserId, request.CurrentEmployeeId, request.Id);
                    throw new UnauthorizedException("You are not authorized to update this profile.");
                }

                // If editing own profile (either Employee or Manager editing themselves)
                if (isSelfUpdate)
                {
                    // Can only edit FirstName and LastName
                    if (!string.IsNullOrWhiteSpace(request.Department) && request.Department != employee.Department)
                        throw new BadRequestException("Employees are not authorized to change their department.");

                    if (request.IsActive.HasValue && request.IsActive.Value != employee.IsActive)
                        throw new BadRequestException("Employees are not authorized to change their status.");

                    if (request.ManagerId.HasValue && request.ManagerId != employee.ManagerId)
                        throw new BadRequestException("Employees are not authorized to change their manager.");

                    if (request.RoleIds != null && request.RoleIds.Any())
                    {
                        var existingRoleIds = employee.EmployeeRoles?.Select(er => er.RoleId).ToList() ?? new List<int>();
                        var rolesChanged = request.RoleIds.Count != existingRoleIds.Count || 
                                           request.RoleIds.Except(existingRoleIds).Any();
                        if (rolesChanged)
                        {
                            throw new BadRequestException("Employees are not authorized to change their roles.");
                        }
                    }
                }
                // If editing a direct report (Manager editing report)
                else if (isDirectReport)
                {
                    // Can edit FirstName, LastName, Department. Can NOT edit ManagerId, IsActive, or RoleIds.
                    if (request.ManagerId.HasValue && request.ManagerId != employee.ManagerId)
                        throw new BadRequestException("Managers are not authorized to change an employee's manager.");

                    if (request.IsActive.HasValue && request.IsActive.Value != employee.IsActive)
                        throw new BadRequestException("Managers are not authorized to change an employee's active status.");

                    if (request.RoleIds != null && request.RoleIds.Any())
                    {
                        var existingRoleIds = employee.EmployeeRoles?.Select(er => er.RoleId).ToList() ?? new List<int>();
                        var rolesChanged = request.RoleIds.Count != existingRoleIds.Count || 
                                           request.RoleIds.Except(existingRoleIds).Any();
                        if (rolesChanged)
                        {
                            throw new BadRequestException("Managers are not authorized to change an employee's roles.");
                        }
                    }
                }
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

            var updatedEmployee = await _repository.GetByIdAsync(employee.Id);
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

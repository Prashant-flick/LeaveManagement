using Employee.Application.DTOs;
using Employee.Application.Interfaces;
using Employee.Application.Common.Exceptions;
using Employee.Domain.Common.Interfaces;
using Employee.Domain.Entities;
using Employee.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Employee.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(
            IEmployeeRepository repository,
            ILogger<EmployeeService> logger,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            _logger.LogInformation(
                "Creating employee for UserId: {UserId}",
                request.UserId);

            var existing = await _repository.GetByUserIdAsync(request.UserId);

            if (existing != null)
            {
                _logger.LogWarning(
                    "Employee already exists for UserId: {UserId}",
                    request.UserId);

                throw new BadRequestException("Employee already exists for this user");
            }

            var employee = new Domain.Entities.Employee
            {
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.AddAsync(employee);

            var roles = request.RoleIds.Select(roleId => new EmployeeRole
            {
                EmployeeId = employee.Id,
                RoleId = roleId
            });

            await _repository.AddEmployeeRolesAsync(roles);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Employee created successfully with Id: {EmployeeId}",
                employee.Id);

            return await GetEmployeeByIdAsync(employee.Id);
        }

        public async Task<List<EmployeeResponse>> GetEmployeesAsync()
        {
            _logger.LogInformation("Fetching all employees");

            var employees = await _repository.GetAllAsync();

            return employees.Select(e => new EmployeeResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Department = e.Department,
                Roles = e.EmployeeRoles?
                    .Select(r => r.Role?.Name ?? "")
                    .ToList() ?? new List<string>()
            }).ToList();
        }

        public async Task<EmployeeResponse> GetEmployeeByIdAsync(int id)
        {
            _logger.LogInformation("Fetching employee by Id: {EmployeeId}", id);

            var e = await _repository.GetByIdAsync(id);

            if (e == null)
            {
                _logger.LogWarning(
                    "Employee not found for Id: {EmployeeId}",
                    id);

                throw new NotFoundException("Employee not found");
            }

            return new EmployeeResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Department = e.Department,
                Roles = e.EmployeeRoles?
                    .Select(r => r.Role?.Name ?? "")
                    .ToList() ?? new List<string>(),
                ManagerId = e.ManagerId
            };
        }

        public async Task<EmployeeResponse> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request)
        {
            _logger.LogInformation("Updating employee with Id: {EmployeeId}", id);

            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
            {
                _logger.LogWarning(
                    "Employee not found for update: {EmployeeId}",
                    id);

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
                _logger.LogInformation(
                    "Updating roles for EmployeeId: {EmployeeId}",
                    id);

                _repository.RemoveEmployeeRoles(employee.EmployeeRoles!);

                var newRoles = request.RoleIds.Select(roleId => new EmployeeRole
                {
                    EmployeeId = employee.Id,
                    RoleId = roleId
                });

                await _repository.AddEmployeeRolesAsync(newRoles);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Employee updated successfully: {EmployeeId}",
                id);

            return await GetEmployeeByIdAsync(id);
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            _logger.LogInformation(
                "Deleting (soft) employee with Id: {EmployeeId}",
                id);

            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
            {
                _logger.LogWarning(
                    "Employee not found for deletion: {EmployeeId}",
                    id);

                throw new NotFoundException("Employee not found");
            }

            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Employee deactivated successfully: {EmployeeId}",
                id);

            return true;
        }

        public async Task<UserRoleResponse> GetRolesAndEmployeeIdByUserId(int userId)
        {
            _logger.LogInformation(
                "Fetching roles for UserId: {UserId}",
                userId);

            var employee = await _repository.GetByUserIdAsync(userId);

            if (employee == null)
            {
                _logger.LogWarning(
                    "Employee not found for UserId: {UserId}",
                    userId);

                throw new NotFoundException("Employee not found for given user");
            }

            var roles = employee.EmployeeRoles!
                .Select(r => r.Role.Name)
                .ToList();

            _logger.LogInformation(
                "Roles fetched for UserId: {UserId}: {Roles}",
                userId,
                string.Join(",", roles));

            return new UserRoleResponse
            {
                EmployeeId = employee.Id,
                Roles = roles
            };
        }
    }
}
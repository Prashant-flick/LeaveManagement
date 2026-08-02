using Employee.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using Employee.Application.Features.Employees.Commands.CreateEmployee;
using Employee.Application.Features.Employees.Commands.DeleteEmployee;
using Employee.Application.Features.Employees.Commands.UpdateEmployee;
using Employee.Application.Features.Employees.Queries.GetEmployeeById;
using Employee.Application.Features.Employees.Queries.GetEmployees;
using Employee.Application.Features.Employees.Queries.GetRolesAndEmployeeIdByUserId;

namespace Employee.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IMediator mediator,
            ILogger<EmployeeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            _logger.LogInformation(
                "Create employee request received for UserId: {UserId}",
                request.UserId);

            var result = await _mediator.Send(new CreateEmployeeCommand(
                request.UserId,
                request.FirstName,
                request.LastName,
                request.Department,
                request.ManagerId,
                request.RoleIds
            ));

            _logger.LogInformation(
                "Employee created successfully for UserId: {UserId}",
                request.UserId);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all employees");

            var result = await _mediator.Send(new GetEmployeesQuery());

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            _logger.LogInformation("Fetching employee by Id: {EmployeeId}", id);

            var result = await _mediator.Send(new GetEmployeeByIdQuery(id));

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
        {
            _logger.LogInformation("Updating employee with Id: {EmployeeId}", id);

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var currentEmployeeId = int.Parse(User.FindFirstValue("EmployeeId") ?? "0");
            var currentUserRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = await _mediator.Send(new UpdateEmployeeCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Department,
                request.IsActive,
                request.ManagerId,
                request.RoleIds,
                currentUserId,
                currentEmployeeId,
                currentUserRoles
            ));

            _logger.LogInformation("Employee updated successfully: {EmployeeId}", id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting employee with Id: {EmployeeId}", id);

            var result = await _mediator.Send(new DeleteEmployeeCommand(id));

            _logger.LogInformation("Employee deleted (soft) successfully: {EmployeeId}", id);

            return Ok(new { message = "Employee deleted successfully" });
        }

        [AllowAnonymous]
        [HttpGet("roles/{userId}")]
        public async Task<IActionResult> GetRolesAndEmployeeId(int userId)
        {
            _logger.LogInformation(
                "Fetching roles and employeeId for UserId: {UserId}",
                userId);

            var result = await _mediator.Send(new GetRolesAndEmployeeIdByUserIdQuery(userId));

            return Ok(result);
        }

        [HttpGet("{id}/manager")]
        [AllowAnonymous]
        public async Task<IActionResult> GetManager(int id)
        {
            _logger.LogInformation(
                "Fetching manager for EmployeeId: {EmployeeId}",
                id);

            var employee = await _mediator.Send(new GetEmployeeByIdQuery(id));

            return Ok(new { ManagerId = employee.ManagerId });
        }
    }
}
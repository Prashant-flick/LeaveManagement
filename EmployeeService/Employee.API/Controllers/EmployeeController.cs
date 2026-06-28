using Employee.Application.DTOs;
using Employee.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeService service,
            ILogger<EmployeeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeRequest request)
        {
            _logger.LogInformation(
                "Create employee request received for UserId: {UserId}",
                request.UserId);

            var result = await _service.CreateEmployeeAsync(request);

            _logger.LogInformation(
                "Employee created successfully for UserId: {UserId}",
                request.UserId);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all employees");

            var result = await _service.GetEmployeesAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            _logger.LogInformation("Fetching employee by Id: {EmployeeId}", id);

            var result = await _service.GetEmployeeByIdAsync(id);

            if (result == null)
            {
                _logger.LogWarning(
                    "Employee not found for Id: {EmployeeId}",
                    id);

                return NotFound(new { message = "Employee not found" });
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateEmployeeRequest request)
        {
            _logger.LogInformation("Updating employee with Id: {EmployeeId}", id);

            var result = await _service.UpdateEmployeeAsync(id, request);

            if (result == null)
            {
                _logger.LogWarning(
                    "Employee not found for update: {EmployeeId}",
                    id);

                return NotFound(new { message = "Employee not found" });
            }

            _logger.LogInformation("Employee updated successfully: {EmployeeId}", id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting employee with Id: {EmployeeId}", id);

            var result = await _service.DeleteEmployeeAsync(id);

            if (!result)
            {
                _logger.LogWarning(
                    "Employee not found for delete: {EmployeeId}",
                    id);

                return NotFound(new { message = "Employee not found" });
            }

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

            var result = await _service.GetRolesAndEmployeeIdByUserId(userId);

            return Ok(result);
        }

        [HttpGet("{id}/manager")]
        [AllowAnonymous]
        public async Task<IActionResult> GetManager(int id)
        {
            _logger.LogInformation(
                "Fetching manager for EmployeeId: {EmployeeId}",
                id);

            var employee = await _service.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                _logger.LogWarning(
                    "Employee not found while fetching manager: {EmployeeId}",
                    id);

                return NotFound(new { message = "Employee not found" });
            }

            return Ok(new { ManagerId = employee.ManagerId });
        }
    }
}
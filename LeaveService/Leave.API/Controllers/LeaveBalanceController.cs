using Leave.API.Extensions;
using Leave.Application.DTOs;
using Leave.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leave.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveBalanceController : ControllerBase
{
    private readonly ILeaveBalanceService _service;
    private readonly ILogger<LeaveBalanceController> _logger;

    public LeaveBalanceController(
        ILeaveBalanceService service,
        ILogger<LeaveBalanceController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateLeaveBalanceRequest request)
    {
        _logger.LogInformation(
            "Received request to create leave balance for EmployeeId: {EmployeeId}",
            request.EmployeeId);

        try
        {
            var result = await _service.CreateBalanceAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating leave balance for EmployeeId: {EmployeeId}",
                request.EmployeeId);

            throw;
        }
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyBalance()
    {
        var employeeId = User.GetEmployeeId();

        _logger.LogInformation(
            "Fetching leave balance for EmployeeId: {EmployeeId}",
            employeeId);

        var result = await _service.GetBalanceByEmployeeIdAsync(employeeId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{employeeId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        _logger.LogInformation(
            "Fetching leave balance for EmployeeId: {EmployeeId}",
            employeeId);

        var result = await _service.GetBalanceByEmployeeIdAsync(employeeId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
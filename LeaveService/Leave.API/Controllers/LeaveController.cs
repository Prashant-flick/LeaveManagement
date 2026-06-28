using Leave.API.Extensions;
using Leave.Application.DTOs;
using Leave.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leave.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _service;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(ILeaveService service, ILogger<LeaveController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> Create(CreateLeaveRequest request)
    {
        var employeeId = User.GetEmployeeId();

        _logger.LogInformation(
            "Received leave creation request from EmployeeId: {EmployeeId}", employeeId);

        request.EmployeeId = employeeId;

        try
        {
            var result = await _service.CreateLeaveAsync(request);

            _logger.LogInformation(
                "Leave created successfully for EmployeeId: {EmployeeId}", employeeId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating leave for EmployeeId: {EmployeeId}",
                employeeId);

            throw; // Let global handler manage response
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> MyLeaves()
    {
        var employeeId = User.GetEmployeeId();

        _logger.LogInformation(
            "Fetching leave requests for EmployeeId: {EmployeeId}", employeeId);

        try
        {
            var result = await _service.GetLeavesByEmployeeAsync(employeeId);

            _logger.LogInformation(
                "Fetched {Count} leave requests for EmployeeId: {EmployeeId}",
                result.Count, employeeId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while fetching leaves for EmployeeId: {EmployeeId}",
                employeeId);

            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Fetching all leave requests (Admin/Manager access)");

        try
        {
            var result = await _service.GetAllLeavesAsync();

            _logger.LogInformation(
                "Fetched {Count} total leave requests",
                result.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching all leave requests");
            throw;
        }
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var approverId = User.GetEmployeeId();
        var isAdmin = User.IsInRole("Admin");

        _logger.LogInformation(
            "Approval request received for LeaveId: {LeaveId} by approverId: {approverId}",
            id, approverId);

        try
        {
            var result = await _service.UpdateLeaveStatusAsync(id, approverId, isAdmin, true);

            if (result == null)
            {
                _logger.LogWarning(
                    "Leave request not found for LeaveId: {LeaveId}", id);

                return NotFound();
            }

            _logger.LogInformation(
                "LeaveId: {LeaveId} processed by ManagerId: {ManagerId}",
                id, approverId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                ex,
                "Unauthorized approval attempt for LeaveId: {LeaveId} by EmployeeId: {EmployeeId}",
                id, approverId);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while approving leave for LeaveId: {LeaveId}",
                id);

            throw;
        }
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Reject(int id)
    {
        var approverId = User.GetEmployeeId();
        var isAdmin = User.IsInRole("Admin");

        _logger.LogInformation(
            "Reject request received for LeaveId: {LeaveId} by approverId: {approverId}",
            id, approverId);

        try
        {
            var result = await _service.UpdateLeaveStatusAsync(id, approverId, isAdmin, false);

            if (result == null)
            {
                _logger.LogWarning(
                    "Leave request not found for LeaveId: {LeaveId}", id);

                return NotFound();
            }

            _logger.LogInformation(
                "LeaveId: {LeaveId} processed by approverId: {approverId}",
                id, approverId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                ex,
                "Unauthorized approval attempt for LeaveId: {LeaveId} by EmployeeId: {EmployeeId}",
                id, approverId);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while approving leave for LeaveId: {LeaveId}",
                id);

            throw;
        }
    }
}
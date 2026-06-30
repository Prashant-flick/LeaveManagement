using Leave.API.Extensions;
using Leave.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Leave.Application.Features.Leaves.Commands.CreateLeave;
using Leave.Application.Features.Leaves.Commands.UpdateLeaveStatus;
using Leave.Application.Features.Leaves.Queries.GetAllLeaves;
using Leave.Application.Features.Leaves.Queries.GetLeavesByEmployee;

namespace Leave.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(IMediator mediator, ILogger<LeaveController> logger)
    {
        _mediator = mediator;
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

        var result = await _mediator.Send(new CreateLeaveCommand(
            employeeId,
            request.StartDate,
            request.EndDate,
            request.Reason
        ));

        _logger.LogInformation(
            "Leave created successfully for EmployeeId: {EmployeeId}", employeeId);

        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> MyLeaves()
    {
        var employeeId = User.GetEmployeeId();

        _logger.LogInformation(
            "Fetching leave requests for EmployeeId: {EmployeeId}", employeeId);

        var result = await _mediator.Send(new GetLeavesByEmployeeQuery(employeeId));

        _logger.LogInformation(
            "Fetched {Count} leave requests for EmployeeId: {EmployeeId}",
            result.Count, employeeId);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Fetching all leave requests (Admin/Manager access)");

        var result = await _mediator.Send(new GetAllLeavesQuery());

        _logger.LogInformation(
            "Fetched {Count} total leave requests",
            result.Count);

        return Ok(result);
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

        var result = await _mediator.Send(new UpdateLeaveStatusCommand(id, approverId, isAdmin, true));

        _logger.LogInformation(
            "LeaveId: {LeaveId} processed by ManagerId: {ManagerId}",
            id, approverId);

        return Ok(result);
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

        var result = await _mediator.Send(new UpdateLeaveStatusCommand(id, approverId, isAdmin, false));

        _logger.LogInformation(
            "LeaveId: {LeaveId} processed by approverId: {approverId}",
            id, approverId);

        return Ok(result);
    }
}
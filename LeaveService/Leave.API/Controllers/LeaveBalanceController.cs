using Leave.API.Extensions;
using Leave.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Leave.Application.Features.LeaveBalances.Commands.CreateLeaveBalance;
using Leave.Application.Features.LeaveBalances.Queries.GetLeaveBalanceByEmployee;

namespace Leave.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveBalanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeaveBalanceController> _logger;

    public LeaveBalanceController(
        IMediator mediator,
        ILogger<LeaveBalanceController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateLeaveBalanceRequest request)
    {
        _logger.LogInformation(
            "Received request to create leave balance for EmployeeId: {EmployeeId}",
            request.EmployeeId);

        var result = await _mediator.Send(new CreateLeaveBalanceCommand(
            request.EmployeeId,
            request.TotalLeaves,
            request.Year
        ));

        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> GetMyBalance()
    {
        var employeeId = User.GetEmployeeId();

        _logger.LogInformation(
            "Fetching leave balance for EmployeeId: {EmployeeId}",
            employeeId);

        var result = await _mediator.Send(new GetLeaveBalanceByEmployeeQuery(employeeId));

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

        var result = await _mediator.Send(new GetLeaveBalanceByEmployeeQuery(employeeId));

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
using Leave.Application.DTOs;
using Leave.Application.Interfaces;
using Leave.Domain.Entities;
using Leave.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Leave.Infrastructure.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly ILeaveRepository _repository;
    private readonly ILogger<LeaveBalanceService> _logger;
    private readonly IEmployeeClient _employeeClient;

    public LeaveBalanceService(
        ILeaveRepository repository,
        ILogger<LeaveBalanceService> logger,
        IEmployeeClient employeeClient)
    {
        _repository = repository;
        _logger = logger;
        _employeeClient = employeeClient;   
    }

    public async Task<LeaveBalance> CreateBalanceAsync(CreateLeaveBalanceRequest request)
    {
        var exists = await _employeeClient.EmployeeExistsAsync(request.EmployeeId);

        if (!exists)
        {
            _logger.LogWarning(
                "Attempt to create leave balance for invalid EmployeeId: {EmployeeId}",
                request.EmployeeId);

            throw new InvalidOperationException("Employee does not exist");
        }

        var existing = await _repository.GetBalanceAsync(request.EmployeeId, request.Year);

        if (existing != null)
            throw new InvalidOperationException("Leave balance already exists for this year");

        var balance = new LeaveBalance
        {
            EmployeeId = request.EmployeeId,
            Year = request.Year,
            TotalLeaves = request.TotalLeaves,
            UsedLeaves = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddBalanceAsync(balance);
        await _repository.SaveChangesAsync();

        return balance;
    }

    public async Task<LeaveBalance?> GetBalanceByEmployeeIdAsync(int employeeId)
    {
        _logger.LogInformation(
            "Fetching leave balance for EmployeeId: {EmployeeId}",
            employeeId);

        var currentYear = DateTime.UtcNow.Year;

        var balance = await _repository.GetBalanceAsync(employeeId, currentYear);

        if (balance == null)
        {
            _logger.LogWarning(
                "Leave balance not found for EmployeeId: {EmployeeId}",
                employeeId);
        }

        return balance;
    }
}
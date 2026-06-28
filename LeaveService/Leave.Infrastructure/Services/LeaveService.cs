using Leave.Application.DTOs;
using Leave.Application.Interfaces;
using Leave.Application.Common.Exceptions;
using Leave.Domain.Entities;
using Leave.Domain.Enums;
using Leave.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Leave.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _repository;
    private readonly IEmployeeClient _employeeClient;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(ILeaveRepository repository, 
        IEmployeeClient employeeClient,
        ILogger<LeaveService> logger)
    {
        _repository = repository;
        _employeeClient = employeeClient;
        _logger = logger;
    }

    public async Task<LeaveResponse> CreateLeaveAsync(CreateLeaveRequest request)
    {
        _logger.LogInformation("Creating leave for EmployeeId: {EmployeeId}", request.EmployeeId);
        if (request.EndDate < request.StartDate)
            throw new BadRequestException("End date cannot be before start date");

        var currentYear = DateTime.UtcNow.Year;

        var balance = await _repository.GetBalanceAsync(request.EmployeeId, currentYear);

        if (balance == null)
            throw new BadRequestException("Leave balance not found");

        int days = (request.EndDate - request.StartDate).Days + 1;
        _logger.LogInformation("Remaining Leaves {} and days {}", balance.RemainingLeaves, days);   
        
        // ✅ NEW LOGIC — fetch existing leaves
        var existingLeaves = await _repository.GetLeavesByEmployeeAndYear(
            request.EmployeeId,
            currentYear
        );

        var alreadyAppliedDays = existingLeaves
            .Where(l => l.Status == LeaveStatus.Pending || l.Status == LeaveStatus.Approved)
            .Sum(l => (l.EndDate - l.StartDate).Days + 1);

        _logger.LogInformation(
            "Already applied days (Pending + Approved): {AppliedDays}",
            alreadyAppliedDays
        );

        if (alreadyAppliedDays + days > balance.TotalLeaves)
        {
            _logger.LogWarning(
                "Insufficient balance. EmployeeId: {EmployeeId}, Requested: {RequestedDays}, AlreadyUsed: {UsedDays}, Total: {TotalLeaves}",
                request.EmployeeId,
                days,
                alreadyAppliedDays,
                balance.TotalLeaves
            );

            throw new BadRequestException("Insufficient leave balance");
        }

        var leave = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
            Reason = request.Reason,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddLeaveAsync(leave);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Leave created successfully for EmployeeId: {EmployeeId}", request.EmployeeId);
        return Map(leave);
    }

    public async Task<List<LeaveResponse>> GetAllLeavesAsync()
    {
        var leaves = await _repository.GetAllAsync();
        return leaves.Select(Map).ToList();
    }

    public async Task<List<LeaveResponse>> GetLeavesByEmployeeAsync(int employeeId)
    {
        var leaves = await _repository.GetByEmployeeAsync(employeeId);
        return leaves.Select(Map).ToList();
    }

    public async Task<LeaveResponse?> UpdateLeaveStatusAsync(int id, int approverId, Boolean isAdmin, Boolean Action)
    {
        _logger.LogInformation("Processing leave approval for LeaveId: {LeaveId}", id);
        var leave = await _repository.GetByIdAsync(id);
        if (leave == null) return null;

        var currentYear = DateTime.UtcNow.Year;
        var balance = await _repository.GetBalanceAsync(leave.EmployeeId, currentYear);

        if (balance == null)
            throw new BadRequestException("Leave balance missing");

        if (leave.Status != LeaveStatus.Pending)
            throw new BadRequestException("Leave already processed");
        
        if (leave.EmployeeId == approverId) {
            _logger.LogWarning("Unauthorized approval attempt by EmployeeId: {EmployeeId}", approverId);
            throw new UnauthorizedException("Employees cannot approve their own leave");
        }

        if (!isAdmin)
        {
            var managerId = await _employeeClient.GetManagerIdAsync(leave.EmployeeId);

            _logger.LogInformation("managerId is {}", managerId);
            if (managerId == null)
                    throw new BadRequestException("Manager not assigned");

            if (managerId != approverId)
                throw new UnauthorizedException("Only reporting manager can approve leave");
        }

        int days = (leave.EndDate - leave.StartDate).Days + 1;

        if (Action)
        {
            if (balance.RemainingLeaves < days)
                throw new BadRequestException("Insufficient balance");

            leave.Status = LeaveStatus.Approved;
            leave.ProcessedBy = approverId;

            balance.UsedLeaves += days;
        }
        else
        {
            leave.Status = LeaveStatus.Rejected;
            leave.ProcessedBy = approverId;
        }

        leave.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Leave approved by ApproverId: {approverId}", approverId);
        return Map(leave);
    }

    private LeaveResponse Map(LeaveRequest l)
    {
        return new LeaveResponse
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Reason = l.Reason,
            Status = l.Status.ToString()
        };
    }
}
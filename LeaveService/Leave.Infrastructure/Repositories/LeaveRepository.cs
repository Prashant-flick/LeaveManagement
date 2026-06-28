using Leave.Domain.Entities;
using Leave.Domain.Interfaces;
using Leave.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leave.Infrastructure.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaveRepository> _logger;

    public LeaveRepository(
        ApplicationDbContext context,
        ILogger<LeaveRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddLeaveAsync(LeaveRequest leave)
    {
        _logger.LogInformation(
            "Adding new leave request for EmployeeId: {EmployeeId}, StartDate: {StartDate}, EndDate: {EndDate}",
            leave.EmployeeId, leave.StartDate, leave.EndDate);

        await _context.LeaveRequests.AddAsync(leave);
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching leave request with Id: {LeaveId}", id);

        var leave = await _context.LeaveRequests
            .FirstOrDefaultAsync(x => x.Id == id);

        if (leave == null)
        {
            _logger.LogWarning("Leave request not found for Id: {LeaveId}", id);
        }

        return leave;
    }

    public async Task<List<LeaveRequest>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all leave requests");

        var result = await _context.LeaveRequests.ToListAsync();

        _logger.LogInformation("Fetched {Count} leave requests", result.Count);

        return result;
    }

    public async Task<List<LeaveRequest>> GetByEmployeeAsync(int employeeId)
    {
        _logger.LogInformation("Fetching leave requests for EmployeeId: {EmployeeId}", employeeId);

        var result = await _context.LeaveRequests
            .Where(x => x.EmployeeId == employeeId)
            .ToListAsync();

        _logger.LogInformation(
            "Fetched {Count} leave requests for EmployeeId: {EmployeeId}",
            result.Count, employeeId);

        return result;
    }

    public async Task<LeaveBalance?> GetBalanceAsync(int employeeId, int year)
    {
        return await _context.LeaveBalances
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Year == year);
    }

    public async Task AddBalanceAsync(LeaveBalance balance)
    {
        _logger.LogInformation(
            "Adding leave balance for EmployeeId: {EmployeeId}, TotalLeaves: {TotalLeaves}",
            balance.EmployeeId, balance.TotalLeaves);

        await _context.LeaveBalances.AddAsync(balance);
    }

    public async Task<List<LeaveRequest>> GetLeavesByEmployeeAndYear(int employeeId, int year)
    {
        return await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId &&
                        l.StartDate.Year == year)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        _logger.LogInformation("Saving changes to database");

        try
        {
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("Database save successful. Rows affected: {RowCount}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes to database");
            throw;
        }
    }
}
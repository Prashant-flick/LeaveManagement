using Leave.Domain.Entities;

namespace Leave.Domain.Interfaces;
public interface ILeaveRepository
{
    Task AddLeaveAsync(LeaveRequest leave);
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task<List<LeaveRequest>> GetAllAsync();
    Task<List<LeaveRequest>> GetByEmployeeAsync(int employeeId);

    Task<LeaveBalance?> GetBalanceAsync(int employeeId, int year);
    Task AddBalanceAsync(LeaveBalance balance);

    Task<List<LeaveRequest>> GetLeavesByEmployeeAndYear(int employeeId, int year);

    Task SaveChangesAsync();
}
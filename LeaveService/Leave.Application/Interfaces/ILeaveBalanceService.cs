using Leave.Application.DTOs;
using Leave.Domain.Entities;

namespace Leave.Application.Interfaces;

public interface ILeaveBalanceService
{
    Task<LeaveBalance> CreateBalanceAsync(CreateLeaveBalanceRequest balance);

    Task<LeaveBalance?> GetBalanceByEmployeeIdAsync(int employeeId);
}
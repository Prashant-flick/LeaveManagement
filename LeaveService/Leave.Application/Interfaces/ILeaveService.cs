using Leave.Application.DTOs;

namespace Leave.Application.Interfaces;
public interface ILeaveService
{
    Task<LeaveResponse> CreateLeaveAsync(CreateLeaveRequest request);
    Task<List<LeaveResponse>> GetAllLeavesAsync();
    Task<List<LeaveResponse>> GetLeavesByEmployeeAsync(int employeeId);

    Task<LeaveResponse?> UpdateLeaveStatusAsync(int id, int approverId, Boolean isAdmin, Boolean Action);
}
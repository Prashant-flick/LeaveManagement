using MediatR;
using Leave.Domain.Entities;

namespace Leave.Application.Features.LeaveBalances.Commands.CreateLeaveBalance
{
    public record CreateLeaveBalanceCommand(
        int EmployeeId,
        int TotalLeaves,
        int Year
    ) : IRequest<LeaveBalance>;
}

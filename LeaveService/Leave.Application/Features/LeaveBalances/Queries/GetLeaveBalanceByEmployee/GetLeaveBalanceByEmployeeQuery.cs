using MediatR;
using Leave.Domain.Entities;

namespace Leave.Application.Features.LeaveBalances.Queries.GetLeaveBalanceByEmployee
{
    public record GetLeaveBalanceByEmployeeQuery(int EmployeeId) : IRequest<LeaveBalance>;
}

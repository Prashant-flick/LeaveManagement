using MediatR;
using Leave.Application.DTOs;

namespace Leave.Application.Features.Leaves.Commands.UpdateLeaveStatus
{
    public record UpdateLeaveStatusCommand(
        int Id,
        int ApproverId,
        bool IsAdmin,
        bool Action
    ) : IRequest<LeaveResponse>;
}

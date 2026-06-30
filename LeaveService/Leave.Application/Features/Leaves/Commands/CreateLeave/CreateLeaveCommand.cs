using MediatR;
using Leave.Application.DTOs;
using System;

namespace Leave.Application.Features.Leaves.Commands.CreateLeave
{
    public record CreateLeaveCommand(
        int EmployeeId,
        DateTime StartDate,
        DateTime EndDate,
        string Reason
    ) : IRequest<LeaveResponse>;
}

using MediatR;
using Leave.Application.DTOs;
using System.Collections.Generic;

namespace Leave.Application.Features.Leaves.Queries.GetLeavesByEmployee
{
    public record GetLeavesByEmployeeQuery(int EmployeeId) : IRequest<List<LeaveResponse>>;
}

using MediatR;
using Leave.Application.DTOs;
using System.Collections.Generic;

namespace Leave.Application.Features.Leaves.Queries.GetAllLeaves
{
    public record GetAllLeavesQuery() : IRequest<List<LeaveResponse>>;
}

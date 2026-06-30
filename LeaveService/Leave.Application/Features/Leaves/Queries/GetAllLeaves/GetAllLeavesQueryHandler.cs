using MediatR;
using Leave.Application.DTOs;
using Leave.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Leave.Application.Features.Leaves.Queries.GetAllLeaves
{
    public class GetAllLeavesQueryHandler : IRequestHandler<GetAllLeavesQuery, List<LeaveResponse>>
    {
        private readonly ILeaveRepository _repository;

        public GetAllLeavesQueryHandler(ILeaveRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LeaveResponse>> Handle(GetAllLeavesQuery request, CancellationToken cancellationToken)
        {
            var leaves = await _repository.GetAllAsync();
            return leaves.Select(l => new LeaveResponse
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status.ToString()
            }).ToList();
        }
    }
}

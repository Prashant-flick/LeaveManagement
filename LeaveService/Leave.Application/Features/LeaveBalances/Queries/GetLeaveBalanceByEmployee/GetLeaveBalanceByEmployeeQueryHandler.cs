using MediatR;
using Leave.Domain.Entities;
using Leave.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leave.Application.Features.LeaveBalances.Queries.GetLeaveBalanceByEmployee
{
    public class GetLeaveBalanceByEmployeeQueryHandler : IRequestHandler<GetLeaveBalanceByEmployeeQuery, LeaveBalance>
    {
        private readonly ILeaveRepository _repository;
        private readonly ILogger<GetLeaveBalanceByEmployeeQueryHandler> _logger;

        public GetLeaveBalanceByEmployeeQueryHandler(
            ILeaveRepository repository,
            ILogger<GetLeaveBalanceByEmployeeQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<LeaveBalance> Handle(GetLeaveBalanceByEmployeeQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching leave balance for EmployeeId: {EmployeeId}", request.EmployeeId);

            var currentYear = DateTime.UtcNow.Year;

            var balance = await _repository.GetBalanceAsync(request.EmployeeId, currentYear);

            if (balance == null)
            {
                _logger.LogWarning("Leave balance not found for EmployeeId: {EmployeeId}", request.EmployeeId);
            }

            return balance!;
        }
    }
}

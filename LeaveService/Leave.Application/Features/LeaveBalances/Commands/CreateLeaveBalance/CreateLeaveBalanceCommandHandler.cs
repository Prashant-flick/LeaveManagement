using MediatR;
using Leave.Application.Common.Exceptions;
using Leave.Domain.Entities;
using Leave.Domain.Interfaces;
using Leave.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leave.Application.Features.LeaveBalances.Commands.CreateLeaveBalance
{
    public class CreateLeaveBalanceCommandHandler : IRequestHandler<CreateLeaveBalanceCommand, LeaveBalance>
    {
        private readonly ILeaveRepository _repository;
        private readonly IEmployeeClient _employeeClient;
        private readonly ILogger<CreateLeaveBalanceCommandHandler> _logger;

        public CreateLeaveBalanceCommandHandler(
            ILeaveRepository repository,
            IEmployeeClient employeeClient,
            ILogger<CreateLeaveBalanceCommandHandler> logger)
        {
            _repository = repository;
            _employeeClient = employeeClient;
            _logger = logger;
        }

        public async Task<LeaveBalance> Handle(CreateLeaveBalanceCommand request, CancellationToken cancellationToken)
        {
            var exists = await _employeeClient.EmployeeExistsAsync(request.EmployeeId);

            if (!exists)
            {
                _logger.LogWarning("Attempt to create leave balance for invalid EmployeeId: {EmployeeId}", request.EmployeeId);
                throw new BadRequestException("Employee does not exist");
            }

            var existing = await _repository.GetBalanceAsync(request.EmployeeId, request.Year);

            if (existing != null)
                throw new BadRequestException("Leave balance already exists for this year");

            var balance = new LeaveBalance
            {
                EmployeeId = request.EmployeeId,
                Year = request.Year,
                TotalLeaves = request.TotalLeaves,
                UsedLeaves = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddBalanceAsync(balance);
            await _repository.SaveChangesAsync();

            return balance;
        }
    }
}

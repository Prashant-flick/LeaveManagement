using MediatR;
using Leave.Application.DTOs;
using Leave.Application.Common.Exceptions;
using Leave.Domain.Entities;
using Leave.Domain.Enums;
using Leave.Domain.Interfaces;
using Leave.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leave.Application.Features.Leaves.Commands.CreateLeave
{
    public class CreateLeaveCommandHandler : IRequestHandler<CreateLeaveCommand, LeaveResponse>
    {
        private readonly ILeaveRepository _repository;
        private readonly IEmployeeClient _employeeClient;
        private readonly ILogger<CreateLeaveCommandHandler> _logger;

        public CreateLeaveCommandHandler(
            ILeaveRepository repository, 
            IEmployeeClient employeeClient,
            ILogger<CreateLeaveCommandHandler> logger)
        {
            _repository = repository;
            _employeeClient = employeeClient;
            _logger = logger;
        }

        public async Task<LeaveResponse> Handle(CreateLeaveCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating leave for EmployeeId: {EmployeeId}", request.EmployeeId);
            if (request.EndDate < request.StartDate)
                throw new BadRequestException("End date cannot be before start date");

            var currentYear = DateTime.UtcNow.Year;

            var balance = await _repository.GetBalanceAsync(request.EmployeeId, currentYear);

            if (balance == null)
                throw new BadRequestException("Leave balance not found");

            int days = (request.EndDate - request.StartDate).Days + 1;
            _logger.LogInformation("Remaining Leaves {RemainingLeaves} and days {Days}", balance.RemainingLeaves, days);   
            
            var existingLeaves = await _repository.GetLeavesByEmployeeAndYear(
                request.EmployeeId,
                currentYear
            );

            var alreadyAppliedDays = existingLeaves
                .Where(l => l.Status == LeaveStatus.Pending || l.Status == LeaveStatus.Approved)
                .Sum(l => (l.EndDate - l.StartDate).Days + 1);

            _logger.LogInformation(
                "Already applied days (Pending + Approved): {AppliedDays}",
                alreadyAppliedDays
            );

            if (alreadyAppliedDays + days > balance.TotalLeaves)
            {
                _logger.LogWarning(
                    "Insufficient balance. EmployeeId: {EmployeeId}, Requested: {RequestedDays}, AlreadyUsed: {UsedDays}, Total: {TotalLeaves}",
                    request.EmployeeId,
                    days,
                    alreadyAppliedDays,
                    balance.TotalLeaves
                );

                throw new BadRequestException("Insufficient leave balance");
            }

            var leave = new LeaveRequest
            {
                EmployeeId = request.EmployeeId,
                StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
                Reason = request.Reason,
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddLeaveAsync(leave);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Leave created successfully for EmployeeId: {EmployeeId}", request.EmployeeId);
            return new LeaveResponse
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status.ToString()
            };
        }
    }
}

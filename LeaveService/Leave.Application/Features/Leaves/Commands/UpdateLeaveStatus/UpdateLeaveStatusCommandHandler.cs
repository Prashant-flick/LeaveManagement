using MediatR;
using Leave.Application.DTOs;
using Leave.Application.Common.Exceptions;
using Leave.Domain.Entities;
using Leave.Domain.Enums;
using Leave.Domain.Interfaces;
using Leave.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leave.Application.Features.Leaves.Commands.UpdateLeaveStatus
{
    public class UpdateLeaveStatusCommandHandler : IRequestHandler<UpdateLeaveStatusCommand, LeaveResponse>
    {
        private readonly ILeaveRepository _repository;
        private readonly IEmployeeClient _employeeClient;
        private readonly ILogger<UpdateLeaveStatusCommandHandler> _logger;

        public UpdateLeaveStatusCommandHandler(
            ILeaveRepository repository, 
            IEmployeeClient employeeClient,
            ILogger<UpdateLeaveStatusCommandHandler> logger)
        {
            _repository = repository;
            _employeeClient = employeeClient;
            _logger = logger;
        }

        public async Task<LeaveResponse> Handle(UpdateLeaveStatusCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing leave approval for LeaveId: {LeaveId}", request.Id);
            var leave = await _repository.GetByIdAsync(request.Id);
            if (leave == null)
            {
                throw new NotFoundException("Leave request not found");
            }

            var currentYear = DateTime.UtcNow.Year;
            var balance = await _repository.GetBalanceAsync(leave.EmployeeId, currentYear);

            if (balance == null)
                throw new BadRequestException("Leave balance missing");

            if (leave.Status != LeaveStatus.Pending)
                throw new BadRequestException("Leave already processed");
            
            if (leave.EmployeeId == request.ApproverId) {
                _logger.LogWarning("Unauthorized approval attempt by EmployeeId: {EmployeeId}", request.ApproverId);
                throw new UnauthorizedException("Employees cannot approve their own leave");
            }

            if (!request.IsAdmin)
            {
                var managerId = await _employeeClient.GetManagerIdAsync(leave.EmployeeId);

                _logger.LogInformation("managerId is {ManagerId}", managerId);
                if (managerId == null)
                        throw new BadRequestException("Manager not assigned");

                if (managerId != request.ApproverId)
                    throw new UnauthorizedException("Only reporting manager can approve leave");
            }

            int days = (leave.EndDate - leave.StartDate).Days + 1;

            if (request.Action)
            {
                if (balance.RemainingLeaves < days)
                    throw new BadRequestException("Insufficient balance");

                leave.Status = LeaveStatus.Approved;
                leave.ProcessedBy = request.ApproverId;

                balance.UsedLeaves += days;
            }
            else
            {
                leave.Status = LeaveStatus.Rejected;
                leave.ProcessedBy = request.ApproverId;
            }

            leave.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync();

            _logger.LogInformation("Leave processed by ApproverId: {ApproverId}", request.ApproverId);
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

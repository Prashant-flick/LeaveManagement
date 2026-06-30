using MediatR;
using Employee.Application.DTOs;
using Employee.Application.Common.Exceptions;
using Employee.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Employee.Application.Features.Employees.Queries.GetEmployeeById
{
    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse>
    {
        private readonly IEmployeeRepository _repository;

        public GetEmployeeByIdQueryHandler(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task<EmployeeResponse> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var e = await _repository.GetByIdAsync(request.Id);

            if (e == null)
            {
                throw new NotFoundException("Employee not found");
            }

            return new EmployeeResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Department = e.Department,
                IsActive = e.IsActive,
                ManagerId = e.ManagerId,
                Roles = e.EmployeeRoles?
                    .Select(r => r.Role?.Name ?? "")
                    .ToList() ?? new List<string>()
            };
        }
    }
}

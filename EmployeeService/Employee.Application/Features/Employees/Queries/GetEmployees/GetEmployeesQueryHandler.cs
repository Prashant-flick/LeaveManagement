using MediatR;
using Employee.Application.DTOs;
using Employee.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Employee.Application.Features.Employees.Queries.GetEmployees
{
    public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeResponse>>
    {
        private readonly IEmployeeRepository _repository;

        public GetEmployeesQueryHandler(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EmployeeResponse>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        {
            var employees = await _repository.GetAllAsync();

            return employees.Select(e => new EmployeeResponse
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
            }).ToList();
        }
    }
}

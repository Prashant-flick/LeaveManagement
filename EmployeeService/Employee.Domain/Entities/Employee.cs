using Employee.Domain.Common;

namespace Employee.Domain.Entities;

public class Employee : BaseEntity
{
    public int UserId { get; set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public required string Department { get; set; }

    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public bool IsActive { get; set; }

    public ICollection<EmployeeRole>? EmployeeRoles { get; set; }
}
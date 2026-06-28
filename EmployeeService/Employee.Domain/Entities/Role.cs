using Employee.Domain.Common;

namespace Employee.Domain.Entities{
    public class Role : BaseEntity
    {
        public required string Name { get; set; }

        public ICollection<EmployeeRole>? EmployeeRoles { get; set; }
    }
}
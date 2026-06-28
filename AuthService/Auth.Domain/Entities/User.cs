using Auth.Domain.Common;

namespace Auth.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public bool IsActive { get; set; }

        public int? EmployeeId { get; set; }
    }
}
namespace Employee.Application.DTOs{
    public class EmployeeResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? ManagerId { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

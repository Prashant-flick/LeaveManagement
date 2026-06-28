namespace Employee.Application.DTOs{
    public class CreateEmployeeRequest
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
}
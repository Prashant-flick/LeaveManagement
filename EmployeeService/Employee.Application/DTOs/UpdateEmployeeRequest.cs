namespace Employee.Application.DTOs{
    public class UpdateEmployeeRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public bool? IsActive { get; set; }
        public int? ManagerId { get; set; }
        public List<int>? RoleIds { get; set; }
    }
}
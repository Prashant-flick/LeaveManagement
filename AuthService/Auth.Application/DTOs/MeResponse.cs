public class MeResponse
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string EmployeeId { get; set; }
    public IEnumerable<string> Roles { get; set; }
}
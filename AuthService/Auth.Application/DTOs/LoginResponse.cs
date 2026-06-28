public class LoginResponse
{
    public int? UserId { get; set; }
    public int? EmployeeId { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }

    public string Token { get; set; }
    public string RefreshToken { get; set; }
}

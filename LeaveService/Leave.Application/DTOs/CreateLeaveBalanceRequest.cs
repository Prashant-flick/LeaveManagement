namespace Leave.Application.DTOs;

public class CreateLeaveBalanceRequest
{
    public int EmployeeId { get; set; }
    public int TotalLeaves { get; set; }
    public int Year { get; set; }
}
namespace Leave.Application.DTOs;
public class CreateLeaveRequest
{
    public int EmployeeId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string Reason { get; set; }
}
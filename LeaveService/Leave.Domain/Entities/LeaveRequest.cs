using Leave.Domain.Common;
using Leave.Domain.Enums;

namespace Leave.Domain.Entities;
public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string Reason { get; set; }

    public LeaveStatus Status { get; set; }

    public int? ProcessedBy { get; set; }
}
using Leave.Domain.Common;

namespace Leave.Domain.Entities;
public class LeaveBalance : BaseEntity
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int TotalLeaves { get; set; }
    public int UsedLeaves { get; set; }

    public int RemainingLeaves => TotalLeaves - UsedLeaves;
}
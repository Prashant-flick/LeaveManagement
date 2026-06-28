using Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leave.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("leave");

        modelBuilder.Entity<LeaveRequest>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<LeaveBalance>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<LeaveRequest>()
            .Property(x => x.Reason)
            .HasMaxLength(500);

        modelBuilder.Entity<LeaveBalance>()
            .HasIndex(x => new { x.EmployeeId, x.Year })
            .IsUnique();
    }
}
using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Employee.Infrastructure.Data{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Domain.Entities.Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("employee");

            modelBuilder.Entity<EmployeeRole>()
                .HasKey(x => new { x.EmployeeId, x.RoleId });

            modelBuilder.Entity<Domain.Entities.Employee>()
                .HasIndex(e => e.UserId)
                .IsUnique();
            
            modelBuilder.Entity<Domain.Entities.Employee>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Role>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
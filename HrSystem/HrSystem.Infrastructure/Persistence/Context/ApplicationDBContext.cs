using Microsoft.EntityFrameworkCore;
using HrSystem.Domain.Entities;

namespace HrSystem.Infrastructure.Persistence.Context
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<EmployeeParticipation> EmployeeParticipations => Set<EmployeeParticipation>();
        public DbSet<GlobalPolicy> GlobalPolicies => Set<GlobalPolicy>();
        public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
        public DbSet<LeavePolicy> LeavePolicies => Set<LeavePolicy>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Self-Referencing Manager Relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany(u => u.DirectReports)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .Property(e => e.Role)
                .IsRequired()
                .HasConversion<string>();

            // User to EmployeeParticipation (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Participation)
                .WithOne(p => p.Employee)
                .HasForeignKey<EmployeeParticipation>(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // User to LeaveRequests (One-to-Many)
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Employee)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveRequest>()
            .Property(e => e.LeaveType)
                .IsRequired()
                .HasConversion<string>();

            modelBuilder.Entity<LeaveRequest>()
            .Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();
                

            // User to LeaveBalances (One-to-Many)
            modelBuilder.Entity<LeaveBalance>()
                .HasOne(lb => lb.Employee)
                .WithMany(u => u.LeaveBalances)
                .HasForeignKey(lb => lb.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            
             modelBuilder.Entity<LeaveBalance>()
            .Property(e => e.LeaveType)
                .IsRequired()
                .HasConversion<string>();

            // LeaveBalance Unique Index per Employee, LeaveType, and Year
            modelBuilder.Entity<LeaveBalance>()
                .HasIndex(lb => new { lb.EmployeeId, lb.LeaveType, lb.Year })
                .IsUnique();

            // LeavePolicy Unique Index for LeaveType
            modelBuilder.Entity<LeavePolicy>()
                .HasIndex(p => p.LeaveType)
                .IsUnique()
                .HasFilter("[IsEnabled] = 1");

             modelBuilder.Entity<LeavePolicy>()
            .Property(e => e.LeaveType)
                .IsRequired()
                .HasConversion<string>();

            // PublicHoliday Date Index
            modelBuilder.Entity<PublicHoliday>()
                .HasIndex(ph => ph.Date)
                .IsUnique();

            // Property Configurations & Precisions
            modelBuilder.Entity<LeaveRequest>()
                .Property(lr => lr.PolicyAllowanceSnapshot)
                .HasPrecision(18, 2);

            // Apply any extra configurations from assembly if added later
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDBContext).Assembly);
        }
    }
}
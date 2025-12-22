using EmployeeManagement.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, long>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<EmployeeSkill> EmployeeSkills { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity table naming (optional but clean)
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<AppRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<long>>().ToTable("UserRoles");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserClaim<long>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<long>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<long>>().ToTable("UserTokens");

            // ==========================
            // Many-to-Many: EmployeeSkill
            // ==========================
            builder.Entity<EmployeeSkill>()
                .HasKey(es => new { es.EmployeeId, es.SkillId });

            builder.Entity<EmployeeSkill>()
                .HasOne(es => es.Employee)
                .WithMany(e => e.EmployeeSkills)
                .HasForeignKey(es => es.EmployeeId);

            builder.Entity<EmployeeSkill>()
                .HasOne(es => es.Skill)
                .WithMany(s => s.EmployeeSkills)
                .HasForeignKey(es => es.SkillId);

            // ==========================
            // Many-to-Many: RolePermission
            // ==========================
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // ==========================
            // One-to-Many: Department -> Employees
            // ==========================
            builder.Entity<Department>()
                .HasMany(d => d.Employees)
                .WithOne(e => e.Department!)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskItem>()
                .HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}

using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Persistence;

public class SmartLeadsDbContext : DbContext
{
    public SmartLeadsDbContext(DbContextOptions<SmartLeadsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeUser> EmployeeUsers => Set<EmployeeUser>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<ColumnFilter> ColumnFilters => Set<ColumnFilter>();
    public DbSet<Variable> Variables => Set<Variable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure all entities to use Guid keys
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var key = entityType.FindPrimaryKey();
            if (key?.Properties.Count == 1 && key.Properties[0].ClrType == typeof(Guid))
            {
                key.Properties[0].ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
            }
        }

        // ---------- Company ----------
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasOne(c => c.ParentCompany)
                .WithMany(c => c.ChildCompanies)
                .HasForeignKey(c => c.ParentCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- UserCompany ----------
        modelBuilder.Entity<UserCompany>(entity =>
        {
            entity.HasIndex(uc => new { uc.UserId, uc.CompanyId }).IsUnique();

            entity.HasOne(uc => uc.User)
                .WithMany(u => u.UserCompanies)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uc => uc.Company)
                .WithMany(c => c.UserCompanies)
                .HasForeignKey(uc => uc.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(uc => uc.Role)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // ---------- Invitation ----------
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasOne(i => i.Company)
                .WithMany(c => c.Invitations)
                .HasForeignKey(i => i.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.InvitedByUser)
                .WithMany(u => u.InvitationsSent)
                .HasForeignKey(i => i.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- NotificationPreference ----------
        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasIndex(np => new { np.UserId, np.NotificationType }).IsUnique();

            entity.HasOne(np => np.User)
                .WithMany()
                .HasForeignKey(np => np.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(np => np.NotificationType)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // ---------- Notification ----------
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(n => n.Company)
                .WithMany()
                .HasForeignKey(n => n.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(n => n.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(n => n.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasIndex(n => new { n.CompanyId, n.UserId });
            entity.HasIndex(n => n.Status);
            entity.HasIndex(n => n.Type);
        });

        // ---------- Employee ----------
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.EmployeeId }).IsUnique();

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Designation)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- EmployeeUser ----------
        modelBuilder.Entity<EmployeeUser>(entity =>
        {
            entity.HasIndex(eu => new { eu.EmployeeId, eu.UserId }).IsUnique();

            entity.HasOne(eu => eu.Employee)
                .WithMany(e => e.EmployeeUsers)
                .HasForeignKey(eu => eu.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(eu => eu.User)
                .WithMany(u => u.EmployeeUsers)
                .HasForeignKey(eu => eu.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Department ----------
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(d => new { d.CompanyId, d.Name }).IsUnique();

            entity.HasOne(d => d.Company)
                .WithMany()
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Designation ----------
        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasIndex(d => new { d.CompanyId, d.Name }).IsUnique();

            entity.HasOne(d => d.Department)
                .WithMany()
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Company)
                .WithMany()
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- ColumnFilter ----------
        modelBuilder.Entity<ColumnFilter>(entity =>
        {
            entity.HasOne(cf => cf.CreatedByUser)
                .WithMany()
                .HasForeignKey(cf => cf.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cf => cf.Company)
                .WithMany()
                .HasForeignKey(cf => cf.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Variable ----------
        modelBuilder.Entity<Variable>(entity =>
        {
            entity.HasOne(v => v.Company)
                .WithMany()
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Add indexes for CompanyId and UserId on all BaseEntity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(BaseEntity).IsAssignableFrom(clrType) && !clrType.IsAbstract)
            {
                var companyIdProp = entityType.FindProperty("CompanyId");
                if (companyIdProp != null)
                    modelBuilder.Entity(clrType).HasIndex("CompanyId");

                var userIdProp = entityType.FindProperty("UserId");
                if (userIdProp != null)
                    modelBuilder.Entity(clrType).HasIndex("UserId");
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseSystemEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

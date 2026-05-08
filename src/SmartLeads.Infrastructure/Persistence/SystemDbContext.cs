using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Persistence;

/// <summary>
/// System database context for managing system-level data.
/// Contains only: Users, Companies, UserCompanies
/// </summary>
public class SystemDbContext : DbContext
{
    public SystemDbContext(DbContextOptions<SystemDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<UserCompany> UserCompanies { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Exclude company-specific entities from system database
        modelBuilder.Ignore<ColumnFilter>();
        modelBuilder.Ignore<Employee>();
        modelBuilder.Ignore<EmployeeUser>();
        modelBuilder.Ignore<Department>();
        modelBuilder.Ignore<Designation>();

        // Configure all entities to use Guid keys
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var key = entityType.FindPrimaryKey();
            if (key?.Properties.Count == 1 && key.Properties[0].ClrType == typeof(Guid))
            {
                key.Properties[0].ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
            }
        }

        // UserCompany - Unique constraint on UserId + CompanyId
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
            
            // Configure Role property as string (convert from enum)
            entity.Property(uc => uc.Role)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // Company self-referencing relationship (hierarchical)
        modelBuilder.Entity<Company>()
            .HasOne(c => c.ParentCompany)
            .WithMany(c => c.ChildCompanies)
            .HasForeignKey(c => c.ParentCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // NotificationPreference configuration
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

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Ignore(n => n.User);

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
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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

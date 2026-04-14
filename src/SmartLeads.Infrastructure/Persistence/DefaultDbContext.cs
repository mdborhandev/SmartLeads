using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Persistence;

/// <summary>
/// Default application database context for contact and related data.
/// </summary>
public class DefaultDbContext : DbContext
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options)
        : base(options)
    {
    }

    // Default database entities
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<ColumnFilter> ColumnFilters { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUser> EmployeeUsers { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Invitation> Invitations { get; set; }

    // Junction tables
    public DbSet<ContactGroup> ContactGroups { get; set; }
    public DbSet<ContactTag> ContactTags { get; set; }

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

        // Keep system entities out of the default database model.
        modelBuilder.Ignore<User>();
        modelBuilder.Ignore<Company>();
        modelBuilder.Ignore<Notification>();
        modelBuilder.Ignore<NotificationPreference>();

        // Configure BaseEntity relationships for all entities
        ConfigureBaseEntityRelationships(modelBuilder);

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.Ignore(c => c.User);
        });

        // Note belongs to Contact
        modelBuilder.Entity<Note>(entity =>
        {
            entity.Ignore(n => n.User);

            entity.HasOne(n => n.Contact)
                .WithMany(c => c.Notes)
                .HasForeignKey(n => n.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Attachment belongs to Contact
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasOne(a => a.Contact)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Many-to-Many: Contact <-> Group
        modelBuilder.Entity<ContactGroup>(entity =>
        {
            entity.HasKey(cg => new { cg.ContactId, cg.GroupId });

            entity.HasOne(cg => cg.Contact)
                .WithMany(c => c.ContactGroups)
                .HasForeignKey(cg => cg.ContactId);

            entity.HasOne(cg => cg.Group)
                .WithMany(g => g.ContactGroups)
                .HasForeignKey(cg => cg.GroupId);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.Ignore(g => g.User);
        });

        // Many-to-Many: Contact <-> Tag
        modelBuilder.Entity<ContactTag>()
            .HasKey(ct => new { ct.ContactId, ct.TagId });

        modelBuilder.Entity<ContactTag>()
            .HasOne(ct => ct.Contact)
            .WithMany(c => c.ContactTags)
            .HasForeignKey(ct => ct.ContactId);

        modelBuilder.Entity<ContactTag>()
            .HasOne(ct => ct.Tag)
            .WithMany(t => t.ContactTags)
            .HasForeignKey(ct => ct.TagId);

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Ignore(t => t.User);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.EmployeeId }).IsUnique();

            // Configure relationships
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Designation)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(d => new { d.CompanyId, d.Name }).IsUnique();
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasIndex(d => new { d.CompanyId, d.Name }).IsUnique();
            
            // Configure relationship with Department
            entity.HasOne(d => d.Department)
                .WithMany()
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeUser>(entity =>
        {
            entity.HasIndex(eu => new { eu.EmployeeId, eu.UserId }).IsUnique();

            entity.HasOne(eu => eu.Employee)
                .WithMany(e => e.EmployeeUsers)
                .HasForeignKey(eu => eu.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(eu => eu.User);
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.Ignore(i => i.Company);
            entity.Ignore(i => i.InvitedByUser);
        });

        modelBuilder.Entity<ColumnFilter>()
            .Ignore(cf => cf.CreatedByUser);
    }

    /// <summary>
    /// Configures indexes for CompanyId and UserId for all BaseEntity types.
    /// Note: No FK relationships to User/Company as they are in SystemDbContext (different database).
    /// </summary>
    private void ConfigureBaseEntityRelationships(ModelBuilder modelBuilder)
    {
        // Get all entity types that inherit from BaseEntity
        var baseEntityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(et => typeof(BaseEntity).IsAssignableFrom(et.ClrType) && !et.ClrType.IsAbstract)
            .ToList();

        foreach (var entityType in baseEntityTypes)
        {
            // Add index for CompanyId
            var companyIdProperty = entityType.FindProperty("CompanyId");
            if (companyIdProperty != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("CompanyId");
            }

            // Add index for UserId
            var userIdProperty = entityType.FindProperty("UserId");
            if (userIdProperty != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("UserId");
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

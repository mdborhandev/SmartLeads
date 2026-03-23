using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Persistence;

/// <summary>
/// Company-specific database context for managing tenant data.
/// Each company has its own database with isolated data for contacts, groups, tags, notes, etc.
/// </summary>
public class CompanyDbContext : DbContext
{
    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options)
    {
    }

    // Company-specific entities
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<ColumnFilter> ColumnFilters { get; set; }

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

        // Contact belongs to User (Owner)
        modelBuilder.Entity<Contact>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note belongs to Contact and User
        modelBuilder.Entity<Note>()
            .HasOne(n => n.Contact)
            .WithMany(c => c.Notes)
            .HasForeignKey(n => n.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Note>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attachment belongs to Contact
        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Contact)
            .WithMany(c => c.Attachments)
            .HasForeignKey(a => a.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // ColumnFilter belongs to User
        modelBuilder.Entity<ColumnFilter>()
            .HasOne(cf => cf.CreatedByUser)
            .WithMany()
            .HasForeignKey(cf => cf.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
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

        return base.SaveChangesAsync(cancellationToken);
    }
}

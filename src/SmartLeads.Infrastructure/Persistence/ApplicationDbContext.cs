using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Company> Companies { get; set; }

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

        // Company self-referencing relationship (hierarchical)
        modelBuilder.Entity<Company>()
            .HasOne(c => c.ParentCompany)
            .WithMany(c => c.ChildCompanies)
            .HasForeignKey(c => c.ParentCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // User belongs to Company
        modelBuilder.Entity<User>()
            .HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Contact belongs to Company and User
        modelBuilder.Entity<Contact>()
            .HasOne(c => c.Company)
            .WithMany(c => c.Contacts)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.User)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-Many: Contact <-> Group
        modelBuilder.Entity<ContactGroup>()
            .HasKey(cg => new { cg.ContactId, cg.GroupId });

        modelBuilder.Entity<ContactGroup>()
            .HasOne(cg => cg.Contact)
            .WithMany(c => c.ContactGroups)
            .HasForeignKey(cg => cg.ContactId);

        modelBuilder.Entity<ContactGroup>()
            .HasOne(cg => cg.Group)
            .WithMany(g => g.ContactGroups)
            .HasForeignKey(cg => cg.GroupId);

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

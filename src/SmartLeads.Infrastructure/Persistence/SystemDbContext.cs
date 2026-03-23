using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Persistence;

/// <summary>
/// System database context for managing system-level data.
/// Contains: Users, Companies, UserCompanies, Employees, EmployeeUsers, Invitations
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
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUser> EmployeeUsers { get; set; }
    public DbSet<Invitation> Invitations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Exclude company-specific entities from system database
        modelBuilder.Ignore<ContactGroup>();
        modelBuilder.Ignore<ContactTag>();
        modelBuilder.Ignore<Contact>();
        modelBuilder.Ignore<Group>();
        modelBuilder.Ignore<Tag>();
        modelBuilder.Ignore<Note>();
        modelBuilder.Ignore<Attachment>();
        modelBuilder.Ignore<ColumnFilter>();

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
        });

        // Employee - Belongs to Company
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.EmployeeId }).IsUnique();

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeeUser - Junction table linking Employees to Users
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

        // Company self-referencing relationship (hierarchical)
        modelBuilder.Entity<Company>()
            .HasOne(c => c.ParentCompany)
            .WithMany(c => c.ChildCompanies)
            .HasForeignKey(c => c.ParentCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invitation belongs to Company and User (InvitedBy)
        modelBuilder.Entity<Invitation>()
            .HasOne(i => i.Company)
            .WithMany(c => c.Invitations)
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invitation>()
            .HasOne(i => i.InvitedByUser)
            .WithMany(u => u.InvitationsSent)
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
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

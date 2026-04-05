namespace SmartLeads.Domain.Models;

/// <summary>
/// Base entity for system-level data (no company association).
/// Used for Users, Companies, and other system-wide entities.
/// </summary>
public abstract class BaseSystemEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Base entity for company-specific data.
/// Used for Contacts, Groups, Tags, Notes, etc. that belong to a specific company database.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public Company Company { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}

// Alias for Halda pattern compatibility
public abstract class BaseModel : BaseEntity
{
}

namespace SmartLeads.Domain.Models;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}

// Alias for Halda pattern compatibility
public abstract class BaseModel : BaseEntity
{
}

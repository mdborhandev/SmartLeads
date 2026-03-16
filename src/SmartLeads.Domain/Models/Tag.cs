namespace SmartLeads.Domain.Models;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }

    // Foreign Key for User (Owner)
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<ContactTag> ContactTags { get; set; } = new List<ContactTag>();
}

public class ContactTag
{
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}

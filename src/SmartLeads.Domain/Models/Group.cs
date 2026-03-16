namespace SmartLeads.Domain.Models;

public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Foreign Key for User (Owner)
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<ContactGroup> ContactGroups { get; set; } = new List<ContactGroup>();
}

public class ContactGroup
{
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;

    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
}

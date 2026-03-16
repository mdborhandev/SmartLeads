namespace SmartLeads.Domain.Models;

public class Contact : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ContactCompany { get; set; }  // The company where the contact works
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    public bool IsArchived { get; set; } = false;

    // Foreign Key for User (Owner)
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Relationships
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<ContactTag> ContactTags { get; set; } = new List<ContactTag>();
    public ICollection<ContactGroup> ContactGroups { get; set; } = new List<ContactGroup>();
}

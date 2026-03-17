namespace SmartLeads.Domain.Models;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Logo { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsParent { get; set; } = false;
    public Guid? ParentCompanyId { get; set; }
    public Company? ParentCompany { get; set; }

    // Child companies (subsidiaries)
    public ICollection<Company> ChildCompanies { get; set; } = new List<Company>();

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}

namespace SmartLeads.Domain.Models;

public class Company : BaseSystemEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Logo { get; set; }
    public bool IsParent { get; set; } = false;
    public Guid? ParentCompanyId { get; set; }
    public Company? ParentCompany { get; set; }

    // Child companies (subsidiaries)
    public ICollection<Company> ChildCompanies { get; set; } = new List<Company>();

    // Navigation properties
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}

using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.Models;

/// <summary>
/// Junction table linking Users to Companies.
/// Tracks which companies a user belongs to, their role in each company, and their default company.
/// </summary>
public class UserCompany : BaseSystemEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public UserRole Role { get; set; } = UserRole.User;  // Role in this specific company
    public bool IsDefault { get; set; } = false;  // Indicates if this is the user's default company

    // Navigation properties
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;
}

namespace SmartLeads.Domain.Models;

/// <summary>
/// Junction table linking Employees to Users.
/// A user can have multiple employee records across different companies.
/// </summary>
public class EmployeeUser : BaseSystemEntity
{
    public Guid EmployeeId { get; set; }
    public Guid UserId { get; set; }
    public bool IsPrimary { get; set; } = false;  // Indicates if this is the primary employee record for the user

    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public User User { get; set; } = null!;
}

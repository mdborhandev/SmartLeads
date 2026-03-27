namespace SmartLeads.Domain.Models;

/// <summary>
/// User account for authentication and basic profile information.
/// Users can be associated with multiple companies through UserCompany.
/// Role is now stored per-company in UserCompany, not at user level.
/// </summary>
public class User : BaseSystemEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePicture { get; set; }

    // Invitation tracking
    public bool IsPasswordSetByUser { get; set; } = false;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiryTime { get; set; }

    // Navigation properties
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public ICollection<EmployeeUser> EmployeeUsers { get; set; } = new List<EmployeeUser>();
    public ICollection<Invitation> InvitationsSent { get; set; } = new List<Invitation>();
}

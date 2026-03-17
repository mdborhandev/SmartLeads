using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.Models;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    // Refresh token support
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Password reset support
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiryTime { get; set; }

    // Navigation properties
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}

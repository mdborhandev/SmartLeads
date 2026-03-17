using System.ComponentModel.DataAnnotations;
using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.DTOs;

public class InviteUserRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public UserRole Role { get; set; } = UserRole.User;

    [Display(Name = "Expiry Days")]
    public int ExpiryDays { get; set; } = 7; // Default 7 days
}

public class AcceptInvitationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
}

public class InvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string InvitedByUserName { get; set; } = string.Empty;
    public DateTime InvitedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsExpired => ExpiresAt < DateTime.UtcNow && !IsAccepted;
}

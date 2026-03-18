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
    
    // Employee Information
    public string? EmployeeId { get; set; }  // Unique employee code (e.g., EMP001)
    public string? Department { get; set; }  // Department name
    public string? Designation { get; set; } // Job title/position
    public string? PhoneNumber { get; set; } // Contact number
    public string? Address { get; set; }     // Address
    public DateTime? DateOfJoining { get; set; } // Joining date
    public bool IsActive { get; set; } = true; // Active/Inactive status
    
    // Invitation tracking
    public bool IsPasswordSetByUser { get; set; } = false; // True if user set their own password via invitation

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

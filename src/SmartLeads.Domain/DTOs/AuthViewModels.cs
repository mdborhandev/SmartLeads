using System.ComponentModel.DataAnnotations;
using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.DTOs;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email or Username")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class UserProfileViewModel
{
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Display(Name = "Role")]
    public UserRole Role { get; set; } = UserRole.User;

    [Display(Name = "Member Since")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CompanyRegistrationViewModel
{
    [Required]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Company Code")]
    public string? CompanyCode { get; set; }

    [EmailAddress]
    [Display(Name = "Company Email")]
    public string? CompanyEmail { get; set; }

    [Display(Name = "Company Phone")]
    public string? CompanyPhone { get; set; }

    [Display(Name = "Company Address")]
    public string? CompanyAddress { get; set; }

    [Required]
    [Display(Name = "Admin Username")]
    public string AdminUsername { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Admin Email")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Admin Password")]
    public string AdminPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Admin Password")]
    [Compare("AdminPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? AdminFirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? AdminLastName { get; set; }
}

public class CreateUserViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Display(Name = "Role")]
    public UserRole Role { get; set; } = UserRole.User;
    
    // Employee Information
    [Display(Name = "Employee ID")]
    public string? EmployeeId { get; set; }
    
    [Display(Name = "Department")]
    public string? Department { get; set; }
    
    [Display(Name = "Designation")]
    public string? Designation { get; set; }
    
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    
    [Display(Name = "Address")]
    public string? Address { get; set; }
    
    [Display(Name = "Date of Joining")]
    public DateTime? DateOfJoining { get; set; }
}

public class EditUserViewModel
{
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Display(Name = "Role")]
    public UserRole Role { get; set; } = UserRole.User;
    
    // Employee Information
    [Display(Name = "Employee ID")]
    public string? EmployeeId { get; set; }
    
    [Display(Name = "Department")]
    public string? Department { get; set; }
    
    [Display(Name = "Designation")]
    public string? Designation { get; set; }
    
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    
    [Display(Name = "Address")]
    public string? Address { get; set; }
    
    [Display(Name = "Date of Joining")]
    public DateTime? DateOfJoining { get; set; }
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}

namespace SmartLeads.Domain.Models;

/// <summary>
/// Employee record within a specific company.
/// Contains company-specific employee information.
/// </summary>
public class Employee : BaseEntity
{
    // Employee Information (specific to this company)
    public string EmployeeId { get; set; } = string.Empty;  // Unique employee code per company (e.g., EMP001)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? NickName { get; set; }
    public string? WorkEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? AlternatePhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }
    public string? NationalIdNumber { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? JoiningType { get; set; }
    public string? EmploymentStatus { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Notes { get; set; }
    
    // Foreign keys
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    
    // Navigation properties
    public Department? Department { get; set; }
    public Designation? Designation { get; set; }
    
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<EmployeeUser> EmployeeUsers { get; set; } = new List<EmployeeUser>();
}

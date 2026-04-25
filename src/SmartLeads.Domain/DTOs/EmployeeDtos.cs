using System.ComponentModel.DataAnnotations;

namespace SmartLeads.Domain.DTOs;

public class EmployeeDto
{
    public Guid? Id { get; set; }
    public string? EmployeeId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? NickName { get; set; }
    public string? WorkEmail { get; set; }
    public string? PersonalEmail { get; set; }
    // Lookup values (store ID)
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }

    // Display text (populated from related tables)
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AlternatePhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Address { get; set; }
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // Original fields (store ID/value)
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }
    public string? JoiningType { get; set; }
    public string? EmploymentStatus { get; set; }
    
    // Display text (populated from Variables table)
    public string? GenderText { get; set; }
    public string? MaritalStatusText { get; set; }
    public string? BloodGroupText { get; set; }
    public string? NationalityText { get; set; }
    public string? JoiningTypeText { get; set; }
    public string? EmploymentStatusText { get; set; }
    
    public string? NationalIdNumber { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
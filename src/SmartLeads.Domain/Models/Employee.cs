namespace SmartLeads.Domain.Models;

/// <summary>
/// Employee record within a specific company.
/// Contains company-specific employee information.
/// </summary>
public class Employee : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Employee Information (specific to this company)
    public string EmployeeId { get; set; } = string.Empty;  // Unique employee code per company (e.g., EMP001)
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<EmployeeUser> EmployeeUsers { get; set; } = new List<EmployeeUser>();
}

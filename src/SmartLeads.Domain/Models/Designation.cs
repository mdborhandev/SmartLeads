namespace SmartLeads.Domain.Models;

/// <summary>
/// Designation (job title/position) within a specific company.
/// Designations are organized under Departments.
/// </summary>
public class Designation : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Foreign key to Department (mandatory cascading relationship)
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    // Navigation property
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

namespace SmartLeads.Domain.Models;

/// <summary>
/// Designation (job title/position) within a specific company.
/// </summary>
public class Designation : BaseEntity
{
    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

namespace SmartLeads.Domain.Models;

/// <summary>
/// Department within a specific company.
/// </summary>
public class Department : BaseEntity
{
    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

namespace SmartLeads.Domain.Models;

/// <summary>
/// Stores variable reference data for a specific company.
/// Can store types like Gender, Religion, Marital Status, Blood Group, etc.
/// with up to 3 additional value columns.
/// </summary>
public class Variable : BaseEntity
{
    public string Type { get; set; } = string.Empty;  // e.g., "Gender", "Religion", "MaritalStatus", "BloodGroup"
    public string Value { get; set; } = string.Empty;  // e.g., "Male", "Female", "Christian", "Muslim"
    public string? Value1 { get; set; }  // Additional column 1
    public string? Value2 { get; set; }  // Additional column 2
    public string? Value3 { get; set; }  // Additional column 3
    public string? Description { get; set; }  // Optional description
    public int SortOrder { get; set; } = 0;  // For ordering
    public bool IsActive { get; set; } = true;  // Active status
}
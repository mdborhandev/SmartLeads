namespace SmartLeads.Domain.DTOs;

public class VariableDto
{
    public Guid? Id { get; set; }

    public string? Type { get; set; }

    public string? Value { get; set; }

    public string? Value1 { get; set; }

    public string? Value2 { get; set; }

    public string? Value3 { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }
}
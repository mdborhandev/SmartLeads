namespace SmartLeads.Domain.DTOs;

/// <summary>
/// DTO for Select2 dropdown options
/// </summary>
public class SelectOptionDto
{
    public string id { get; set; } = string.Empty;
    public string text { get; set; } = string.Empty;
    public bool selected { get; set; }
    public int? serial { get; set; }
}

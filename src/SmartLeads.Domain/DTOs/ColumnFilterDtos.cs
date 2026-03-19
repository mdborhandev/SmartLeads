namespace SmartLeads.Domain.DTOs;

public class ColumnFilterRequest
{
    public string KeyValue { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class ColumnFilterResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ColumnFilterDto? Data { get; set; }
}

public class ColumnFilterDto
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? ListName { get; set; }
    public string? KeyValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ColumnFilterRemove
{
    public string Type { get; set; } = string.Empty;
}

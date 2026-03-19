using System.Text.Json.Serialization;

namespace SmartLeads.Domain.DTOs;

public class PaginationRequest
{
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;
    
    [JsonPropertyName("size")]
    public int PageSize { get; set; } = 10;
    
    [JsonPropertyName("search")]
    public string? Search { get; set; }
    
    [JsonPropertyName("sort")]
    public string? SortField { get; set; }
    
    [JsonPropertyName("dir")]
    public string? SortOrder { get; set; } // "asc" or "desc"
}

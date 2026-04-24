using System.Text.Json.Serialization;

namespace SmartLeads.Domain.DTOs;

public class PaginationRequest
{
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;
    
    // Also support lowercase "page" from Tabulator
    public int page { get; set; } = 1;
    
    [JsonPropertyName("size")]
    public int PageSize { get; set; } = 10;
    
    // Also support "page_size" and "size"
    public int size { get; set; } = 10;
    public int page_size { get; set; } = 10;
    
    [JsonPropertyName("search")]
    public string? Search { get; set; }
    
    // Tabulator parameters
    [JsonPropertyName("sort")]
    public string? SortField { get; set; }
    
    [JsonPropertyName("dir")]
    public string? SortOrder { get; set; } // "asc" or "desc"
    
    // Also support standard names
    [JsonPropertyName("SortField")]
    public string? SortFieldAlt { get; set; }
    
    [JsonPropertyName("SortOrder")]
    public string? SortOrderAlt { get; set; }
    
    // Get effective values (support both naming conventions)
    public string? GetSortField() => SortField ?? SortFieldAlt;
    public string? GetSortOrder() => SortOrder ?? SortOrderAlt;
    
    // Get effective page values
    public int GetPage() => Page > 0 ? Page : (page > 0 ? page : 1);
    public int GetPageSize() => PageSize > 0 ? PageSize : (size > 0 ? size : (page_size > 0 ? page_size : 10));
}

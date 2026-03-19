using System.Text.Json.Serialization;

namespace SmartLeads.Domain.DTOs;

public class PaginationResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();
    
    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }
    
    [JsonPropertyName("total")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }
}

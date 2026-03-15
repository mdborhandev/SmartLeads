namespace SmartLeads.Domain.Models;

public class Note : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public int ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
}

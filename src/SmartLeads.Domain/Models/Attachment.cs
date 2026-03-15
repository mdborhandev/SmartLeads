namespace SmartLeads.Domain.Models;

public class Attachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;

    public int ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
}

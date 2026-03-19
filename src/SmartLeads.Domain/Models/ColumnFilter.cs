namespace SmartLeads.Domain.Models;

public class ColumnFilter : BaseModel
{
    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public string? ListName { get; set; }
    public string? KeyValue { get; set; }
}

using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.Models;

public class Invitation : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public Guid InvitedByUserId { get; set; }
    public User? InvitedByUser { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; } = false;
    public DateTime? AcceptedAt { get; set; }
    public string? RejectedReason { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Rejected,
    Expired,
    Cancelled
}

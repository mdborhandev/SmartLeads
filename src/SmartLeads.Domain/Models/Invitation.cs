using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.Models;

/// <summary>
/// Invitation sent by a company to invite users.
/// </summary>
public class Invitation : BaseSystemEntity
{
    public string Email { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public Guid InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; } = false;
    public DateTime? AcceptedAt { get; set; }
    public string? RejectedReason { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string? Metadata { get; set; }  // Additional info as JSON
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Rejected,
    Expired,
    Cancelled
}

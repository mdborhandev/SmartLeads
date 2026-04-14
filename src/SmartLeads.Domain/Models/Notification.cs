using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.Models;

/// <summary>
/// Notification entity for in-app and email notifications.
/// Notifications are company-specific and belong to a user.
/// </summary>
public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    
    // Optional reference to related entity (e.g., TaskId, ContactId, InvitationId)
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; } // e.g., "Task", "Contact", "Invitation"
    
    // Action URL for navigation
    public string? ActionUrl { get; set; }
    
    // Email notification tracking
    public bool IsEmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }
    
    // Metadata
    public string? Metadata { get; set; } // JSON for additional data
    
    // Navigation properties
    public User User { get; set; } = null!;
}

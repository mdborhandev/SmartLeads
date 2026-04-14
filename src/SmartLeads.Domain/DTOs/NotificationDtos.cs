using System.ComponentModel.DataAnnotations;
using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.DTOs;

// DTOs for Notification
public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsEmailSent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateNotificationRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    public NotificationType Type { get; set; }

    public Guid? RelatedEntityId { get; set; }
    
    [StringLength(100)]
    public string? RelatedEntityType { get; set; }
    
    [StringLength(500)]
    public string? ActionUrl { get; set; }
    
    public string? Metadata { get; set; }
    
    public bool SendEmail { get; set; } = false;
}

public class NotificationListResponse
{
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
}

public class NotificationStatsDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int ReadCount { get; set; }
    public int ArchivedCount { get; set; }
    public Dictionary<NotificationType, int> CountByType { get; set; } = new();
}

// DTOs for Notification Preferences
public class NotificationPreferenceDto
{
    public Guid Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public bool EnableInApp { get; set; }
    public bool EnableEmail { get; set; }
}

public class UpdateNotificationPreferenceRequest
{
    [Required]
    public NotificationType NotificationType { get; set; }

    public bool EnableInApp { get; set; }
    public bool EnableEmail { get; set; }
}

public class NotificationPreferencesViewModel
{
    public List<NotificationPreferenceDto> Preferences { get; set; } = new();
    public bool SaveSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

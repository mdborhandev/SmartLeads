using SmartLeads.Domain.Enums;

namespace SmartLeads.Domain.Models;

/// <summary>
/// Notification preferences for user settings.
/// Controls which notification types are enabled for in-app and email.
/// </summary>
public class NotificationPreference : BaseSystemEntity
{
    public Guid UserId { get; set; }
    public NotificationType NotificationType { get; set; }
    
    // Preference flags
    public bool EnableInApp { get; set; } = true;
    public bool EnableEmail { get; set; } = true;
    
    // Navigation properties
    public User User { get; set; } = null!;
}

using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;

namespace SmartLeads.Utilities.Interfaces;

/// <summary>
/// Notification preference repository interface for use by INotificationService.
/// Full implementation lives in Infrastructure.
/// </summary>
public interface INotificationPreferenceRepo
{
    Task<NotificationPreference?> GetByUserIdAndTypeAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default);
    Task<IList<NotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdatePreferenceAsync(Guid userId, NotificationType type, bool enableInApp, bool enableEmail, CancellationToken cancellationToken = default);
    Task InitializeDefaultPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}

using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface INotificationPreferenceRepository : IBaseRepository<NotificationPreference, Guid>
{
    Task<NotificationPreference?> GetByUserIdAndTypeAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default);
    Task<IList<NotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Business logic methods
    Task UpdatePreferenceAsync(Guid userId, NotificationType type, bool enableInApp, bool enableEmail, CancellationToken cancellationToken = default);
    Task InitializeDefaultPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}

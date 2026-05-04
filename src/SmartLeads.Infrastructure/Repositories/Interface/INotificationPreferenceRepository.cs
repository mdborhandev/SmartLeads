using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface INotificationPreferenceRepository : IBaseRepository<NotificationPreference, Guid>, SmartLeads.Utilities.Interfaces.INotificationPreferenceRepository
{
    new Task<NotificationPreference?> GetByUserIdAndTypeAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default);
    new Task<IList<NotificationPreference>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Business logic methods
    new Task UpdatePreferenceAsync(Guid userId, NotificationType type, bool enableInApp, bool enableEmail, CancellationToken cancellationToken = default);
    new Task InitializeDefaultPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}

using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;

namespace SmartLeads.Infrastructure.Repositories.Interface;

public interface INotificationRepository : IBaseRepository<Notification, Guid>
{
    Task<IList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IList<Notification>> GetByTypeAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Business logic methods
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsUnreadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task ArchiveAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task ArchiveAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteOldNotificationsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}

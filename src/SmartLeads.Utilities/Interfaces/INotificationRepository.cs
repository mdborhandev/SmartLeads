using SmartLeads.Domain.Models;

namespace SmartLeads.Utilities.Interfaces;

/// <summary>
/// Notification repository interface for use by INotificationService.
/// Full implementation lives in Infrastructure.
/// </summary>
public interface INotificationRepository
{
    // Core CRUD
    Task AddAsync(Notification entity, CancellationToken token = default);
    Task EditAsync(Notification entity);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken token = default);

    // Query methods
    Task<IList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Business logic methods
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

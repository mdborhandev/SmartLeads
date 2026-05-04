using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class NotificationRepository : BaseRepository<Notification, Guid>, INotificationRepository
{
    private readonly SystemDbContext _systemDbContext;

    public NotificationRepository(SystemDbContext dbContext) : base(dbContext)
    {
        _systemDbContext = dbContext;
    }

    public async Task<IList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _systemDbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _systemDbContext.Notifications
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<Notification>> GetByTypeAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default)
    {
        return await _systemDbContext.Notifications
            .Where(n => n.UserId == userId && n.Type == type && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _systemDbContext.Notifications
            .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread && !n.IsDeleted, cancellationToken);
    }

    // Business logic methods
    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(notificationId, cancellationToken);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Read;
            await EditAsync(notification);
            await SaveAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _systemDbContext.Notifications
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.Status = NotificationStatus.Read;
        }

        await SaveAsync();
    }

    public async Task MarkAsUnreadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(notificationId, cancellationToken);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Unread;
            await EditAsync(notification);
            await SaveAsync();
        }
    }

    public async Task ArchiveAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdAsync(notificationId, cancellationToken);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Archived;
            await EditAsync(notification);
            await SaveAsync();
        }
    }

    public async Task ArchiveAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _systemDbContext.Notifications
            .Where(n => n.UserId == userId && n.Status != NotificationStatus.Archived && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.Status = NotificationStatus.Archived;
        }

        await SaveAsync();
    }

    public async Task DeleteOldNotificationsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var oldNotifications = await _systemDbContext.Notifications
            .Where(n => n.CreatedAt < olderThan && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var notification in oldNotifications)
        {
            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.UtcNow;
        }

        await SaveAsync();
    }
}

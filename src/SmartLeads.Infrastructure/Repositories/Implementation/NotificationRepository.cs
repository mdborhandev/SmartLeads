using Microsoft.EntityFrameworkCore;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Persistence;
using SmartLeads.Infrastructure.Repositories.Interface;

namespace SmartLeads.Infrastructure.Repositories.Implementation;

public class NotificationRepository : BaseRepository<Notification, Guid>, INotificationRepository
{
    private new readonly SmartLeadsDbContext _dbContext;

    public NotificationRepository(SmartLeadsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread, cancellationToken);
    }

    public async Task<IList<Notification>> GetByTypeAsync(Guid userId, NotificationType type, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId && n.Type == type)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Read;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.Status = NotificationStatus.Read;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsUnreadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Unread;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ArchiveAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Archived;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ArchiveAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && n.Status != NotificationStatus.Archived)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.Status = NotificationStatus.Archived;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOldNotificationsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var oldNotifications = await _dbContext.Notifications
            .Where(n => n.CreatedAt < olderThan)
            .ToListAsync(cancellationToken);

        if (oldNotifications.Any())
        {
            _dbContext.Notifications.RemoveRange(oldNotifications);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

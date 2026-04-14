using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;

namespace SmartLeads.Utilities.Interfaces;

/// <summary>
/// Service for creating and managing notifications.
/// Use this service to create notifications from controllers, services, or handlers.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Create a notification for a user.
    /// </summary>
    Task<NotificationDto> CreateNotificationAsync(
        Guid userId,
        Guid companyId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null,
        string? metadata = null,
        bool sendEmail = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create notifications for multiple users (broadcast).
    /// </summary>
    Task CreateNotificationsForMultipleUsersAsync(
        IEnumerable<Guid> userIds,
        Guid companyId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null,
        string? metadata = null,
        bool sendEmail = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a notification as read. Returns false if not found or not owned by user.
    /// </summary>
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark all notifications as read for a user. Returns the previous unread count.
    /// </summary>
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notifications for a user with optional filtering and pagination.
    /// </summary>
    Task<NotificationListResponse> GetNotificationsAsync(
        Guid userId,
        string? type = null,
        string? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unread notification count for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notification preferences for a user. Initializes defaults if none exist.
    /// </summary>
    Task<List<NotificationPreferenceDto>> GetNotificationPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a notification preference for a user.
    /// </summary>
    Task<NotificationPreferenceDto> UpdateNotificationPreferenceAsync(
        Guid userId,
        NotificationType notificationType,
        bool enableInApp,
        bool enableEmail,
        CancellationToken cancellationToken = default);
}

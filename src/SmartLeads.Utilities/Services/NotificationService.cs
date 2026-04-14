using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Utilities.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepo _notificationRepository;
    private readonly INotificationPreferenceRepo _notificationPreferenceRepository;
    private readonly IUserRepo _userRepository;
    private readonly IEmailService _emailService;

    public NotificationService(
        INotificationRepo notificationRepository,
        INotificationPreferenceRepo notificationPreferenceRepository,
        IUserRepo userRepository,
        IEmailService emailService)
    {
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        Guid userId, Guid companyId, string title, string message,
        NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null,
        string? actionUrl = null, string? metadata = null, bool sendEmail = false,
        CancellationToken cancellationToken = default)
    {
        bool shouldSendEmail = sendEmail;

        if (shouldSendEmail)
        {
            var preference = await _notificationPreferenceRepository
                .GetByUserIdAndTypeAsync(userId, type, cancellationToken);

            shouldSendEmail = preference?.EnableEmail ?? true;
        }

        var notification = new Notification
        {
            UserId = userId,
            CompanyId = companyId,
            Title = title,
            Message = message,
            Type = type,
            Status = NotificationStatus.Unread,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            ActionUrl = actionUrl,
            Metadata = metadata
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);

        if (shouldSendEmail)
        {
            await SendNotificationEmailAsync(userId, notification, cancellationToken);
        }

        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Status = notification.Status,
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType,
            ActionUrl = notification.ActionUrl,
            IsEmailSent = notification.IsEmailSent,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt
        };
    }

    public async Task CreateNotificationsForMultipleUsersAsync(
        IEnumerable<Guid> userIds, Guid companyId, string title, string message,
        NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null,
        string? actionUrl = null, string? metadata = null, bool sendEmail = false,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            await CreateNotificationAsync(
                userId, companyId, title, message, type,
                relatedEntityId, relatedEntityType, actionUrl, metadata,
                sendEmail, cancellationToken);
        }
    }

    public async Task<bool> MarkAsReadAsync(
        Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null || notification.UserId != userId)
            return false;

        await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);
        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        return unreadCount;
    }

    public async Task<NotificationListResponse> GetNotificationsAsync(
        Guid userId, string? type = null, string? status = null,
        int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);

        // Filter by type if specified
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, out var parsedType))
        {
            notifications = notifications.Where(n => n.Type == parsedType).ToList();
        }

        // Filter by status if specified
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, out var parsedStatus))
        {
            notifications = notifications.Where(n => n.Status == parsedStatus).ToList();
        }

        var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);

        var notificationDtos = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                Status = n.Status,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                ActionUrl = n.ActionUrl,
                IsEmailSent = n.IsEmailSent,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToList();

        return new NotificationListResponse
        {
            Notifications = notificationDtos,
            UnreadCount = unreadCount,
            TotalCount = notifications.Count
        };
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);
    }

    public async Task<List<NotificationPreferenceDto>> GetNotificationPreferencesAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await _notificationPreferenceRepository.GetByUserIdAsync(userId, cancellationToken);

        if (!preferences.Any())
        {
            await _notificationPreferenceRepository.InitializeDefaultPreferencesAsync(userId, cancellationToken);
            preferences = await _notificationPreferenceRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        return preferences.Select(p => new NotificationPreferenceDto
        {
            Id = p.Id,
            NotificationType = p.NotificationType,
            EnableInApp = p.EnableInApp,
            EnableEmail = p.EnableEmail
        }).ToList();
    }

    public async Task<NotificationPreferenceDto> UpdateNotificationPreferenceAsync(
        Guid userId, NotificationType notificationType,
        bool enableInApp, bool enableEmail, CancellationToken cancellationToken = default)
    {
        await _notificationPreferenceRepository.UpdatePreferenceAsync(
            userId, notificationType, enableInApp, enableEmail, cancellationToken);

        var preference = await _notificationPreferenceRepository
            .GetByUserIdAndTypeAsync(userId, notificationType, cancellationToken);

        if (preference == null)
            throw new Exception("Failed to update preference");

        return new NotificationPreferenceDto
        {
            Id = preference.Id,
            NotificationType = preference.NotificationType,
            EnableInApp = preference.EnableInApp,
            EnableEmail = preference.EnableEmail
        };
    }

    private async Task SendNotificationEmailAsync(
        Guid userId, Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var emailSubject = $"[SmartLeads] {notification.Title}";
                var emailBody = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #333;'>{notification.Title}</h2>
                            <p>{notification.Message}</p>
                            <p style='color: #666; font-size: 12px;'>
                                Received at: {DateTime.UtcNow:MMMM dd, yyyy HH:mm} UTC
                            </p>
                            <p style='color: #999; font-size: 11px;'>
                                You can manage your notification preferences in your profile settings.
                            </p>
                        </body>
                    </html>";

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                notification.IsEmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _notificationRepository.EditAsync(notification);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email notification: {ex.Message}");
        }
    }
}

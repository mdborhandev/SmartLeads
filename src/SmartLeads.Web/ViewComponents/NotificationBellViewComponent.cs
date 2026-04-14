using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.ViewComponents;

public class NotificationBellViewComponent : ViewComponent
{
    private readonly INotificationService _notificationService;

    public NotificationBellViewComponent(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int maxItems = 5)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return View(new NotificationBellViewModel());
        }

        var claimsPrincipal = User as System.Security.Claims.ClaimsPrincipal;
        var userId = Guid.Parse(claimsPrincipal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Get unread count
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        // Get recent notifications
        var recentNotifications = await _notificationService.GetNotificationsAsync(userId, page: 1, pageSize: maxItems);

        var viewModel = new NotificationBellViewModel
        {
            UnreadCount = unreadCount,
            RecentNotifications = recentNotifications.Notifications
        };

        return View(viewModel);
    }
}

public class NotificationBellViewModel
{
    public int UnreadCount { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; } = new();
}

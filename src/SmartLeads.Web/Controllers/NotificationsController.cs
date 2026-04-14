using Microsoft.AspNetCore.Mvc;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Controllers;

public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationsController(INotificationService notificationService, IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    // GET: Notifications
    [HttpGet]
    public async Task<IActionResult> Index(string? type, string? status, int page = 1)
    {
        var response = await _notificationService.GetNotificationsAsync(UserId, type, status, page, 20);

        ViewBag.UnreadCount = response.UnreadCount;
        ViewBag.TotalCount = response.TotalCount;
        ViewBag.CurrentPage = page;
        ViewBag.SelectedType = type;
        ViewBag.SelectedStatus = status;

        return View(response);
    }

    // GET: Notifications/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var notification = await _unitOfWork.notificationRepository.GetByIdAsync(id);

        if (notification == null || notification.UserId != UserId)
        {
            return NotFound();
        }

        // Mark as read when viewed
        if (notification.Status == Domain.Enums.NotificationStatus.Unread)
        {
            await _notificationService.MarkAsReadAsync(id, UserId);
        }

        var dto = new NotificationDto
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

        return View(dto);
    }

    // POST: Notifications/MarkAsRead/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(id, UserId);

        if (!result)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Notifications/MarkAllAsRead
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(UserId);

        return RedirectToAction(nameof(Index));
    }

    // POST: Notifications/Archive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id)
    {
        var notification = await _unitOfWork.notificationRepository.GetByIdAsync(id);

        if (notification == null || notification.UserId != UserId)
        {
            return NotFound();
        }

        await _unitOfWork.notificationRepository.ArchiveAsync(id);

        return RedirectToAction(nameof(Index));
    }

    // GET: Notifications/UnreadCount
    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(UserId);

        return Json(new { count });
    }

    // GET: Notifications/GetRecent
    [HttpGet]
    public async Task<IActionResult> GetRecent(int count = 10)
    {
        var response = await _notificationService.GetNotificationsAsync(UserId, page: 1, pageSize: count);

        return Json(response.Notifications);
    }

    // GET: Notifications/Preferences
    [HttpGet]
    public async Task<IActionResult> Preferences()
    {
        var preferences = await _notificationService.GetNotificationPreferencesAsync(UserId);

        var viewModel = new NotificationPreferencesViewModel
        {
            Preferences = preferences
        };

        return View(viewModel);
    }

    // POST: Notifications/Preferences
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preferences(NotificationPreferencesViewModel viewModel)
    {
        try
        {
            foreach (var preference in viewModel.Preferences)
            {
                await _notificationService.UpdateNotificationPreferenceAsync(
                    UserId,
                    preference.NotificationType,
                    preference.EnableInApp,
                    preference.EnableEmail);
            }

            viewModel.SaveSuccess = true;
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }

        return View(viewModel);
    }

    // API: POST api/notifications
    [HttpPost("api/notifications")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateNotificationApi([FromBody] CreateNotificationRequest request)
    {
        try
        {
            var result = await _notificationService.CreateNotificationAsync(
                UserId,
                CompanyId,
                request.Title,
                request.Message,
                request.Type,
                request.RelatedEntityId,
                request.RelatedEntityType,
                request.ActionUrl,
                request.Metadata,
                request.SendEmail);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // API: DELETE api/notifications/old
    [HttpDelete("api/notifications/old")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteOldNotifications([FromQuery] int daysOld = 90)
    {
        try
        {
            var olderThan = DateTime.UtcNow.AddDays(-daysOld);
            await _unitOfWork.notificationRepository.DeleteOldNotificationsAsync(olderThan);

            return Ok(new { message = "Old notifications deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

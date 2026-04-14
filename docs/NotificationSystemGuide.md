# Notification System - Usage Guide

## Overview
The SmartLeads Notification System provides:
- In-app notifications with a bell icon in the navbar
- Email notifications (configurable per user)
- Notification preferences per user
- Multiple notification categories (Task, Email, System, Alert, Invitation, Contact)
- Mark as read/unread functionality
- Archive and delete operations

## Architecture

### Domain Layer
- **Notification** entity (`SmartLeads.Domain.Models.Notification`)
- **NotificationPreference** entity (`SmartLeads.Domain.Models.NotificationPreference`)
- **NotificationType** enum: Task, Email, System, Alert, Invitation, Contact
- **NotificationStatus** enum: Unread, Read, Archived

### Infrastructure Layer
- **INotificationRepository** & **INotificationPreferenceRepository**
- **NotificationService** (`INotificationService`) - Easy-to-use service for creating notifications
- Registered in DI container automatically

### Web Layer
- **NotificationsController** - Handles all notification routes
- **NotificationBellViewComponent** - Renders notification bell in navbar
- MediatR Commands/Queries for CQRS pattern
- Views: Index, Details, Preferences

## How to Create Notifications

### Method 1: Using INotificationService (Recommended)

Inject `INotificationService` into your controller or handler:

```csharp
public class ContactController : BaseController
{
    private readonly INotificationService _notificationService;
    
    public async Task<IActionResult> CreateContact(Contact model)
    {
        // ... save contact ...
        
        // Create notification for the current user
        await _notificationService.CreateNotificationAsync(
            userId: UserId,
            companyId: CompanyId,
            title: "Contact Created",
            message: $"Contact '{model.Name}' has been created successfully.",
            type: NotificationType.Contact,
            relatedEntityId: contact.Id,
            relatedEntityType: "Contact",
            actionUrl: Url.Action("Details", "Contacts", new { id = contact.Id }),
            sendEmail: true
        );
        
        return RedirectToAction("Index");
    }
}
```

### Method 2: Using MediatR Commands

```csharp
using SmartLeads.Web.Notifications.Commands.CreateNotification;

// In your controller/handler
var command = new CreateNotificationCommand(
    UserId: userId,
    CompanyId: companyId,
    Title: "New Task Assigned",
    Message: "You have been assigned a new task: Follow up with client",
    Type: NotificationType.Task,
    RelatedEntityId: taskId,
    RelatedEntityType: "Task",
    ActionUrl: "/tasks/123",
    SendEmail: true
);

var result = await _mediator.Send(command);
```

### Method 3: Direct Repository Access

```csharp
var notification = new Notification
{
    UserId = userId,
    CompanyId = companyId,
    Title = "System Update",
    Message = "System maintenance scheduled for tonight.",
    Type = NotificationType.System,
    Status = NotificationStatus.Unread
};

await _unitOfWork.notificationRepository.AddAsync(notification);
await _unitOfWork.SaveAsync();
```

## Broadcasting to Multiple Users

```csharp
// Notify all admins about a system event
var adminUserIds = await GetAdminUserIds(); // Your logic here

await _notificationService.CreateNotificationsForMultipleUsersAsync(
    userIds: adminUserIds,
    companyId: companyId,
    title: "System Alert",
    message: "Database backup completed successfully.",
    type: NotificationType.Alert,
    sendEmail: false
);
```

## Notification Preferences

Users can manage their notification preferences at `/Notifications/Preferences`:

- **EnableInApp**: Show notifications in the app bell icon
- **EnableEmail**: Send email notifications

Preferences are checked automatically when creating notifications.

## Routes

- `GET /Notifications` - List all notifications with filters
- `GET /Notifications/Details/{id}` - View notification details
- `GET /Notifications/Preferences` - Manage notification preferences
- `POST /Notifications/MarkAsRead/{id}` - Mark single notification as read
- `POST /Notifications/MarkAllAsRead` - Mark all notifications as read
- `POST /Notifications/Archive/{id}` - Archive a notification
- `GET /Notifications/UnreadCount` - API endpoint for unread count
- `GET /Notifications/GetRecent?count=10` - API endpoint for recent notifications

## Notification Bell Component

The notification bell is automatically rendered in the navbar layout. It shows:
- Unread count badge
- Recent notifications (last 5)
- Links to view all notifications and settings

To refresh the bell via JavaScript (e.g., after creating a notification):

```javascript
// Reload the navbar or use SignalR for real-time updates
location.reload(); // Simple approach
```

## Email Notifications

Email notifications:
- Use the existing `IEmailService` 
- Send HTML formatted emails
- Track email sent status in the notification entity
- Respect user preferences

## Examples by Notification Type

### Task Notification
```csharp
await _notificationService.CreateNotificationAsync(
    userId: assigneeId,
    companyId: CompanyId,
    title: "New Task Assigned",
    message: "You have a new task: Review Q2 reports",
    type: NotificationType.Task,
    relatedEntityId: taskId,
    relatedEntityType: "Task",
    actionUrl: $"/tasks/{taskId}",
    sendEmail: true
);
```

### Contact Notification
```csharp
await _notificationService.CreateNotificationAsync(
    userId: UserId,
    companyId: CompanyId,
    title: "New Contact Added",
    message: "John Doe has been added to your contacts",
    type: NotificationType.Contact,
    relatedEntityId: contactId,
    relatedEntityType: "Contact",
    actionUrl: $"/contacts/details/{contactId}",
    sendEmail: false
);
```

### Invitation Notification
```csharp
await _notificationService.CreateNotificationAsync(
    userId: inviterUserId,
    companyId: CompanyId,
    title: "Invitation Accepted",
    message: "john@example.com has accepted your invitation",
    type: NotificationType.Invitation,
    relatedEntityId: invitationId,
    relatedEntityType: "Invitation",
    sendEmail: true
);
```

### System Notification
```csharp
await _notificationService.CreateNotificationAsync(
    userId: UserId,
    companyId: CompanyId,
    title: "Password Changed",
    message: "Your password was successfully changed",
    type: NotificationType.System,
    sendEmail: true
);
```

### Alert Notification
```csharp
await _notificationService.CreateNotificationAsync(
    userId: UserId,
    companyId: CompanyId,
    title: "Storage Limit Warning",
    message: "You have used 90% of your storage limit",
    type: NotificationType.Alert,
    sendEmail: true
);
```

## Database Migration

After implementing the notification system, create and apply migrations:

```bash
# For SystemDbContext (NotificationPreference)
dotnet ef migrations add AddNotificationPreferences --context SystemDbContext --output-dir Migrations/SystemDb

# For DefaultDbContext (Notification)
dotnet ef migrations add AddNotifications --context DefaultDbContext --output-dir Migrations/DefaultDb

# Apply migrations
dotnet ef database update --context SystemDbContext
dotnet ef database update --context DefaultDbContext
```

## Future Enhancements

- Real-time notifications with SignalR
- Notification sounds
- Bulk operations UI
- Notification templates
- Scheduled notifications
- Push notifications (browser/mobile)

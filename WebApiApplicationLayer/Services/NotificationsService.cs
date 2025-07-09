using DomainLayer.Constants;
using DomainLayer.Entities.General;
using InfrastructureLayer;
using Microsoft.AspNetCore.SignalR;
using WebApiApplicationLayer.Interfaces;

namespace WebApiApplicationLayer.Services;

public class NotificationService(IHubContext<NotificationsHub, INotificationHub> hubContext,ApplicationDbContext context) : INotificationsService
{
    public async Task SendNotificationAsync(string forUserId, string fromUserId, Consts.NotificationTypes notificationType, string targetId)
    {
        bool notificationExists = NotificationExists(forUserId, fromUserId, notificationType, targetId);
        if (notificationExists)
            return;
        UserNotification userNotification = new UserNotification()
        {
            ForUserId = forUserId,
            FromUserId = fromUserId,
            NotificationTypeId = (int)notificationType,
            TargetId = targetId,
            CreatedAt = DateTime.UtcNow,
        };
        context.UserNotifications.Add(userNotification);
        await context.SaveChangesAsync();
        await hubContext.Clients.User(forUserId).NotificationPing();
    }

    public bool NotificationExists(string forUserId, string fromUserId, Consts.NotificationTypes notificationType, string targetId)
    {
        return context.UserNotifications.Any(x => x.FromUserId == fromUserId
                                                  && x.ForUserId == forUserId
                                                  && x.TargetId == targetId
                                                  && x.NotificationTypeId == (int)notificationType);
    }
}
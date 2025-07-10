using ApplicationLayer.Interfaces;
using DomainLayer.Constants;

namespace WebApiApplicationLayer.Interfaces;

public interface IServerNotificationsService : ISharedNotificationsService
{
    public Task SendNotificationAsync(string forUserId, string fromUserId, Consts.NotificationTypes notificationType, string targetId);
    protected bool NotificationExists(string forUserId, string fromUserId, Consts.NotificationTypes notificationType, string targetId);
}
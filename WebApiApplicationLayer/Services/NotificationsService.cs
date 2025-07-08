using Microsoft.AspNetCore.SignalR;
using WebApiApplicationLayer.Interfaces;

namespace WebApiApplicationLayer.Services;

public class NotificationService(IHubContext<NotificationsHub, INotificationHub> hubContext) : INotificationsService
{
    public async Task SendNotificationAsync(string userId)
    {
        await hubContext.Clients.User(userId).NotificationPing();
    }
}
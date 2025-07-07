using ApplicationLayer.ExtensionClasses;
using DomainLayer.Constants;
using Microsoft.AspNetCore.SignalR;

namespace WebApiApplicationLayer;

public class NotificationsHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    } 
    public async Task SendNotificationCount(string userId)
    {
        await Clients.User(userId).NotificationPing();
    }
}

public interface INotificationClient
{
    Task NotificationPing();
}
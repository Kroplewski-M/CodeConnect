using Microsoft.AspNetCore.SignalR;

namespace WebApiApplicationLayer.Services;

public class NotificationsHub : Hub<INotificationHub>
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    } 
}
public interface INotificationHub
{
    Task NotificationPing();
}
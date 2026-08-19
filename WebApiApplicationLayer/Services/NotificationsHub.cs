using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApiApplicationLayer.Services;
[Authorize]
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
namespace WebApiApplicationLayer.Interfaces;

public interface INotificationsService
{
    public Task SendNotificationAsync(string userId);
}
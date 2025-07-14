using ApplicationLayer.Interfaces;

namespace ClientApplicationLayer.Interfaces;

public interface IClientNotificationsService : ISharedNotificationsService
{
    public event Action? OnNotificationCountChanged;
    public void AddNotification();
}
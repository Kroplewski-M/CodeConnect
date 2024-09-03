namespace ApplicationLayer;

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info,
}

public record Notification(string Message, NotificationType NotificationType);

public class NotificationsService
{
    public NotificationsService()
    {
        Timer = new Timer(RemoveOldestNotification, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
    }
    public List<Notification> Notifications = [];
    public Timer Timer;

    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    public void PushNotification(Notification notification)
    {
        Notifications.Add(notification);
        NotifyStateChanged();
    }

    public void RemoveOldestNotification(object? state)
    {
        if (Notifications.Any())
        {
            Notifications.RemoveAt(0);
            NotifyStateChanged();
        }
    }

    public List<Notification> GetNotification() => Notifications;
}

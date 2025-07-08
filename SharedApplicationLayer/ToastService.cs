namespace ApplicationLayer;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info,
}

public record Toast(string Message, ToastType ToastType);

public class ToastService
{
    public ToastService()
    {
        Timer = new Timer(RemoveOldestToast, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
    }
    private List<Toast> _toasts = [];
    public Timer Timer;

    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    public void PushToast(Toast toast)
    {
        _toasts.Add(toast);
        NotifyStateChanged();
    }

    private void RemoveOldestToast(object? state)
    {
        if (_toasts.Any())
        {
            _toasts.RemoveAt(0);
            NotifyStateChanged();
        }
    }

    public List<Toast> GetToasts() => _toasts;
}

using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ClientApplicationLayer.Interfaces;

namespace ClientApplicationLayer.Services;

public class NotificationsServiceClient(HttpClient httpClient) : IClientNotificationsService
{
    public event Action? OnNotificationCountChanged;
    private int? NotificationsCount { get; set; } = null;
    public void AddNotification()
    {
        NotificationsCount++;
        NotifyStateChanged();
    }
    private void NotifyStateChanged() => OnNotificationCountChanged?.Invoke();
    private string? CurrentUserId { get; set; }
    public async Task<int> GetUsersNotificationsCount(string? userId = null)
    {
        if (NotificationsCount != null && CurrentUserId == userId) 
            return NotificationsCount.Value;
        var response = await httpClient.GetFromJsonAsync<int?>($"api/Notifications/GetNotificationsCount");
        NotificationsCount = response;
        CurrentUserId = userId;
        return NotificationsCount ?? 0;
    }

    public async Task<GetNotificationsDto> GetUsersNotifications(string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<GetNotificationsDto>($"api/Notifications/GetNotifications");
        if (response == null)
            return new GetNotificationsDto(false, new List<NotificationsDto>());
        return response;
    }

    public async Task<ServiceResponse> MarkNotificationAsRead(Guid notificationId, string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync("api/Notifications/MarkNotificationAsRead", notificationId);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        if (result == null)
            return new ServiceResponse(false, "An error occured while marking notification as read");
        if (NotificationsCount is not > 0 || !result.Flag) 
            return result;
        NotificationsCount--;
        NotifyStateChanged();
        return result;
    }

    public async Task<ServiceResponse> MarkAllNotificationsAsRead(string? userId = null)
    {
        var response = await httpClient.PostAsync("api/Notifications/MarkAllNotificationsAsRead", null);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        if (result == null)
            return new ServiceResponse(false, "An error occured while marking notification as read");
        if (result.Flag)
        {
            NotificationsCount = 0;
            NotifyStateChanged();
        }
        return result;
    }
}
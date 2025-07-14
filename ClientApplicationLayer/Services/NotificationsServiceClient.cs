using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ClientApplicationLayer.Interfaces;

namespace ClientApplicationLayer.Services;

public class NotificationsServiceClient(HttpClient httpClient) : IClientNotificationsService
{
    public async Task<int> GetUsersNotificationsCount(string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<int?>($"api/Notifications/GetNotificationsCount");
        return response ?? 0;
    }

    public async Task<GetNotificationsDto> GetUsersNotifications(string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<GetNotificationsDto>($"api/Notifications/GetNotifications");
        if (response == null)
            return new GetNotificationsDto(false, new List<NotificationsDto>());
        return response;
    }
}
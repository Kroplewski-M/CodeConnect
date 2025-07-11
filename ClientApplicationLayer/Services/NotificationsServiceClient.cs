using System.Net.Http.Json;
using ClientApplicationLayer.Interfaces;

namespace ClientApplicationLayer.Services;

public class NotificationsServiceClient(HttpClient httpClient) : IClientNotificationsService
{
    public async Task<int> GetUsersNotificationsCount(string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<int?>($"api/Notifications/GetNotificationsCount");
        return response ?? 0;
    }
}
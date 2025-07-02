using System.Net.Http.Json;
using ApplicationLayer;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;

namespace ClientApplicationLayer;

public class FollowingServiceClient(HttpClient httpClient, NotificationsService notificationsService) : IFollowingService
{
    public async Task<FollowerCount> GetUserFollowersCount(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<FollowerCount>($"api/Following/UserFollowersCount?username={userName}");
        if(response == null)
            notificationsService.PushNotification(new Notification("An Error Occured when fetching followers", NotificationType.Error));
        return response ?? new FollowerCount(0,0);
    }

    public async Task<ServiceResponse> FollowUser(FollowRequestDto followRequest, string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync<FollowRequestDto>("api/Following/FollowUser", followRequest);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
            if (result != null)
                return result;
        }
        notificationsService.PushNotification(new Notification("An Error Occured when following user", NotificationType.Error));
        return new ServiceResponse(false, "Error Occured");
    }

    public async Task<ServiceResponse> UnfollowUser(FollowRequestDto unFollowRequest, string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync<FollowRequestDto>("api/Following/UnFollowUser",unFollowRequest);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
            if (result != null)
                return result;
        }
        notificationsService.PushNotification(new Notification("An Error Occured when unfollowing user", NotificationType.Error));
        return new ServiceResponse(false, "Error Occured");
    }

    public async Task<bool> IsUserFollowing(FollowRequestDto request, string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<bool>(
            $"api/Following/IsUserFollowing?targetUsername={request.TargetUsername}");
        return response;
    }

    public async Task<List<UserBasicDto>> GetUserFollowers(string username, int skip, int take)
    {
        var response = await httpClient.GetFromJsonAsync<List<UserBasicDto>>($"api/Following/GetUserFollowers?username={username}&Skip={skip}&Take={take}");
        return response ?? new List<UserBasicDto>();
    }

    public async Task<List<UserBasicDto>> GetUserFollowing(string username,int skip, int take)
    {
        var response = await httpClient.GetFromJsonAsync<List<UserBasicDto>>($"api/Following/GetUserFollowing?username={username}&Skip={skip}&Take={take}");
        return response ?? new List<UserBasicDto>();
    }
}
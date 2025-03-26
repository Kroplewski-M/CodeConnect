using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;

namespace ApplicationLayer.ClientServices;

public class FollowingServiceClient(HttpClient httpClient, NotificationsService notificationsService) : IFollowingService
{
    public async Task<FollowerCount> GetUserFollowersCount(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<FollowerCount>($"api/Following/UserFollowersCount?username={userName}");
        if(response == null)
            notificationsService.PushNotification(new Notification("An Error Occured when fetching followers", NotificationType.Error));
        return response ?? new FollowerCount(0,0);
    }

    public async Task<ServiceResponse> FollowUser(FollowRequestDto followRequest)
    {
        var response = await httpClient.PostAsJsonAsync<FollowRequestDto>("api/Following/FollowUser", followRequest);
        if(response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<ServiceResponse>())!;
        notificationsService.PushNotification(new Notification("An Error Occured when following user", NotificationType.Error));
        return new ServiceResponse(false, "Error Occured");
    }

    public async Task<ServiceResponse> UnfollowUser(FollowRequestDto unFollowRequest)
    {
        var response = await httpClient.PostAsJsonAsync<FollowRequestDto>("api/Following/UnFollowUser",unFollowRequest);
        if(response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<ServiceResponse>())!;
        notificationsService.PushNotification(new Notification("An Error Occured when unfollowing user", NotificationType.Error));
        return new ServiceResponse(false, "Error Occured");
    }

    public async Task<bool> IsUserFollowing(FollowRequestDto request)
    {
        var response = await httpClient.GetFromJsonAsync<bool>(
            $"api/Following/IsUserUnfollowing?currentUsername={request.CurrentUsername}&targetUsername={request.TargetUsername}");
        return response;
    }

    public async Task<List<UserBasicDto>> GetUserFollowers(string username)
    {
        var response = await httpClient.GetFromJsonAsync<List<UserBasicDto>>($"api/Following/GetUserFollowers?Username={username}");
        return response ?? new List<UserBasicDto>();
    }

    public async Task<List<UserBasicDto>> GetUserFollowing(string username)
    {
        var response = await httpClient.GetFromJsonAsync<List<UserBasicDto>>($"api/Following/GetUserFollowing?username={username}");
        return response ?? new List<UserBasicDto>();
    }
}
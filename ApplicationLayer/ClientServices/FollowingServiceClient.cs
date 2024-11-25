using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;

namespace ApplicationLayer.ClientServices;

public class FollowingServiceClient(HttpClient httpClient, NotificationsService notificationsService) : IFollowingService
{
    public async Task<FollowerCount> GetUserFollowers(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<FollowerCount>($"api/Following/UserFollowing?username={userName}");
        if(response == null)
            notificationsService.PushNotification(new Notification("An Error Occured when fetching followers", NotificationType.Error));
        return response;
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

    public async Task<UserBasicDto> GetUserFollowersProfile(string username)
    {
        throw new NotImplementedException();
    }

    public async Task<UserBasicDto> GetUserFollowingProfile(string username)
    {
        throw new NotImplementedException();
    }
}
using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;

namespace ApplicationLayer.ClientServices;

public class FollowingServiceClient(HttpClient httpClient) : IFollowingService
{
    public async Task<FollowerCount> GetUserFollowers(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<FollowerCount>($"api/Following/UserFollowing?username={userName}");
        if(response == null)
            throw new UnauthorizedAccessException();
        return response;
    }

    public async Task<ServiceResponse> FollowUser(FollowRequestDto followRequest)
    {
        var response = await httpClient.PostAsJsonAsync<FollowRequestDto>("api/Following/FollowUser", followRequest);
        if(response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<ServiceResponse>())!;
        return new ServiceResponse(false, "Error Occured");
    }

    public async Task<ServiceResponse> UnfollowUser(FollowRequestDto unFollowRequest)
    {
        var response = await httpClient.PostAsJsonAsync<FollowRequestDto>("api/Following/FollowUser",unFollowRequest);
        if(response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<ServiceResponse>())!;
        return new ServiceResponse(false, "Error Occured");
    }

    public async Task<bool> IsUserFollowing(FollowRequestDto request)
    {
        var response = await httpClient.GetFromJsonAsync<bool>(
            $"api/Following/IsUserFollowing?currentUsername={request.CurrentUsername}&targetUsername={request.TargetUsername}");
        return response;
    }
}
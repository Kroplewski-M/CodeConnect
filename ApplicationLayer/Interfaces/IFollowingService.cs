using ApplicationLayer.DTO_s;

namespace ApplicationLayer.Interfaces;

public interface IFollowingService
{
    public Task<FollowerCount> GetUserFollowers(string username);
    public Task<ServiceResponse> FollowUser(FollowRequestDto followRequest);
    public Task<ServiceResponse> UnfollowUser(FollowRequestDto unFollowRequest);
}
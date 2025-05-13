using ApplicationLayer.DTO_s;

namespace ApplicationLayer.Interfaces;

public interface IFollowingService
{
    public Task<FollowerCount> GetUserFollowersCount(string username);
    public Task<ServiceResponse> FollowUser(FollowRequestDto followRequest);
    public Task<ServiceResponse> UnfollowUser(FollowRequestDto unFollowRequest);
    public Task<bool> IsUserFollowing(FollowRequestDto request);
    public Task<List<UserBasicDto>> GetUserFollowers(string username, int skip, int take);
    public Task<List<UserBasicDto>> GetUserFollowing(string username);

}
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;

namespace ApplicationLayer.Interfaces;

public interface IFollowingService
{
    public Task<FollowerCount> GetUserFollowersCount(string username);
    public Task<ServiceResponse> FollowUser(FollowRequestDto followRequest, string? userId = null);
    public Task<ServiceResponse> UnfollowUser(FollowRequestDto unFollowRequest, string? userId = null);
    public Task<bool> IsUserFollowing(FollowRequestDto request, string? userId = null);
    public Task<List<UserBasicDto>> GetUserFollowers(string username, int skip, int take);
    public Task<List<UserBasicDto>> GetUserFollowing(string username,  int skip, int take);

}
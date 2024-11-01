using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.User;

public class Followers
{
    public int Id { get; set; }
    public required string FollowerUserId { get; set; }
    public required ApplicationUser Follower { get; set; }
    public required string FollowedUserId { get; set; }
    public required ApplicationUser Followed { get; set; }
}
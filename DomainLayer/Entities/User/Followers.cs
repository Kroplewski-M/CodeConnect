using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.User;

public class Followers
{
    public int Id { get; set; }
    public required string FollowerUserId { get; set; }
    public ApplicationUser? Follower { get; init; }
    public required string FollowedUserId { get; set; }
    public ApplicationUser? Followed { get; init; }
}
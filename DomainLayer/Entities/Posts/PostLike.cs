using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.Posts;

public class PostLike
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public required string LikedByUserId { get; set; }
    public ApplicationUser? LikedByUser { get; set; }
    public DateTime LikedOn { get; set; }
}
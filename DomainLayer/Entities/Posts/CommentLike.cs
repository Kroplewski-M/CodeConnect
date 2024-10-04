using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.Posts;

public class CommentLike
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public Comment? Comment { get; set; }
    public required string LikedByUserId { get; set; }
    public ApplicationUser? LikedByUser { get; set; }
    public DateTime LikedOn { get; set; }
}
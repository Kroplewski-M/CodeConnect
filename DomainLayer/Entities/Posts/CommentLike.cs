using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.Posts;

public class CommentLike
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }
    public required string LikedByUserId { get; set; }
    public ApplicationUser? LikedByUser { get; set; }
    public DateTime LikedOn { get; set; }
}
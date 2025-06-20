using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.Posts;

public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}
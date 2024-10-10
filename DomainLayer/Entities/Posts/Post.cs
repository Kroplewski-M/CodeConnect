using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.Posts;

public class Post
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public string? FileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedByUserId { get; set; }
    public required ApplicationUser CreatedByUser { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public ICollection<PostFile>Files { get; set; } = new List<PostFile>();
}
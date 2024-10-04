using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.Posts;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
}
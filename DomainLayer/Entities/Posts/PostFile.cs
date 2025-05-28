namespace DomainLayer.Entities.Posts;

public class PostFile
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    public required string FileName { get; set; }
}
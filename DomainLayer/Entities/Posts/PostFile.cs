namespace DomainLayer.Entities.Posts;

public class PostFile
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public required string FileName { get; set; }
}
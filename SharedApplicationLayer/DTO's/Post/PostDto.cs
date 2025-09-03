namespace ApplicationLayer.DTO_s.Post;

public record PostDto(Guid Id, string Content, string CreatedByUsername,string UserImg, int CommentCount, int LikeCount,List<string>Images, DateTime CreatedAtUtc);
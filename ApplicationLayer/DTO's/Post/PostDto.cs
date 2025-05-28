namespace ApplicationLayer.DTO_s.Post;

public record PostDto(Guid Id,string Content, List<string>ImageNames, string CreatedBy,DateTime CreatedAt, int LikeCount, int CommentCount);
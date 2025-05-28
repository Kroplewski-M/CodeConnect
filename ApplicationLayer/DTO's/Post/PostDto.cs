namespace ApplicationLayer.DTO_s.Post;

public record PostDto(int Id,string Content, List<string>ImageNames, string CreatedBy,DateTime CreatedAt, int LikeCount, int CommentCount);
namespace ApplicationLayer.DTO_s;

public record CommentDto(int Id,string Content, UserBasicDto CreatedBy, int LikeCount, DateTime CreatedAt);
using ApplicationLayer.DTO_s.User;

namespace ApplicationLayer.DTO_s.Post;

public record CommentDto(int Id,string Content, UserBasicDto CreatedBy, int LikeCount, DateTime CreatedAt);
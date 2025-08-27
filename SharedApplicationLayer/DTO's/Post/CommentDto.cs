using ApplicationLayer.DTO_s.User;

namespace ApplicationLayer.DTO_s.Post;

public record CommentDto(Guid Id,string Content, UserBasicDto CreatedBy, int LikeCount, DateTime CreatedAt, bool CurrentUserLikes);

public record UpsertCommentDto(bool Flag,string Message, CommentDto? Comment);
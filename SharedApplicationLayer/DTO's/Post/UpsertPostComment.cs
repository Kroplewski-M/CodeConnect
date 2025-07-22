namespace ApplicationLayer.DTO_s.Post;

public record UpsertPostComment(Guid PostId,Guid? CommentId, string Content);
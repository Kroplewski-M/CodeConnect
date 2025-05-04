namespace ApplicationLayer.DTO_s;

public record PostDto(int Id,string Content, List<string>ImageNames, string CreatedBy,DateTime CreatedAt, List<CommentDto> Comments, int LikeCount);
namespace ApplicationLayer.DTO_s;

public record PostDto(int Id,string Content, List<string>ImageNames, List<CommentDto> Comments, int LikeCount);
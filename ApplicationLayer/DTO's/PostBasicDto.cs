namespace ApplicationLayer.DTO_s;

public record PostBasicDto(int Id, string Content, string CreatedByUsername,string UserImg, int CommentCount, int LikeCount,List<string>Images, DateTime CreatedAtUtc);
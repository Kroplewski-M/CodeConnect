namespace ApplicationLayer.DTO_s;

public record PostBasicDto(int Id, string Content, string CreatedByUsername,string userImg, int CommentCount, int LikeCount,List<string>Images, DateTime CreatedAtUtc);
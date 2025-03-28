namespace ApplicationLayer.DTO_s;

public record CreatePostDto(string Content, List<Base64Dto>? Images, string CreatedByUser);
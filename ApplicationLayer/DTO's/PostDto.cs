namespace ApplicationLayer.DTO_s;

public record PostDto(string Content, List<Base64Dto>? Images, string CreatedByUser);
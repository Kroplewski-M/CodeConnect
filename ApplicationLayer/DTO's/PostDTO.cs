namespace ApplicationLayer.DTO_s;

public record PostDTO(string Content, List<string>? Images, string CreatedByUser);
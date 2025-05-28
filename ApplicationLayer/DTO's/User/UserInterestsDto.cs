namespace ApplicationLayer.DTO_s.User;

public record UserInterestsDto(bool Flag, string Message,List<TechInterestsDto>? Interests);

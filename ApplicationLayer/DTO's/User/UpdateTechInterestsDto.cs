namespace ApplicationLayer.DTO_s.User;

public record UpdateTechInterestsDto(string UserId, List<TechInterestsDto> Interests);
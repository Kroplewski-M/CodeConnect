namespace ApplicationLayer.DTO_s.User;

public record UpdateTechInterestsDto(string Username, List<TechInterestsDto> Interests);
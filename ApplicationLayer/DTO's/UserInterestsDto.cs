using DomainLayer.DbEnts;

namespace ApplicationLayer.DTO_s;

public record UserInterestsDto(bool flag, string message,List<TechInterestsDto>? Interests);

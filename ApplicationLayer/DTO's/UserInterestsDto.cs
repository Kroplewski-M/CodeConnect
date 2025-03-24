using DomainLayer.DbEnts;

namespace ApplicationLayer.DTO_s;

public record UserInterestsDto(bool Flag, string Message,List<TechInterestsDto>? Interests);

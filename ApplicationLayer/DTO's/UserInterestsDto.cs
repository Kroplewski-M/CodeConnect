using DomainLayer.DbEnts;

namespace ApplicationLayer.DTO_s;

public record UserInterestsDto(bool flag, string message,List<TechInterests>? Interests);

//TODO CHANGE TECHINTERESTS TO A DTO AS THE JSON IS NOT MAPPING THEM CORRECTLY
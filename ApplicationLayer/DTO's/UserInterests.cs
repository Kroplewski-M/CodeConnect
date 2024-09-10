using DomainLayer.DbEnts;

namespace ApplicationLayer.DTO_s;

public record UserInterests(bool flag, string message,List<TechInterests>? Interests);
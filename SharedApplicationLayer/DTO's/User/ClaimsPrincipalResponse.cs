using System.Security.Claims;

namespace ApplicationLayer.DTO_s.User;

public record ClaimsPrincipalResponse(bool Flag, ClaimsPrincipal ClaimsPrincipal);
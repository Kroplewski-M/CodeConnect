using System.Security.Claims;
namespace ApplicationLayer.DTO_s;

public record ClaimsPrincipalResponse(bool Flag, ClaimsPrincipal ClaimsPrincipal);
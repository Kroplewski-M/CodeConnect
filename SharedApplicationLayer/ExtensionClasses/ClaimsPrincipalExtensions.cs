using System.Security.Claims;
using DomainLayer.Constants;

namespace ApplicationLayer.ExtensionClasses;

public static class ClaimsPrincipalExtensions
{
    public static string? GetInfo(this ClaimsPrincipal user, string type)
    {
        return user.FindFirst(type)?.Value;
    }
}
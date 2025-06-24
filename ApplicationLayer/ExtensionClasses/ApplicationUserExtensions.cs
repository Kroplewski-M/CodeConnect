using System.Globalization;
using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;

namespace ApplicationLayer.ExtensionClasses;

public static class ApplicationUserExtensions
{
    public static List<Claim> GetJwtClaimsForUser(this ApplicationUser user)
    {
        return
        [
            new Claim(Consts.ClaimTypes.Id, user.Id),
            new Claim(Consts.ClaimTypes.FirstName, user.FirstName ?? ""),
            new Claim(Consts.ClaimTypes.LastName, user.LastName ?? ""),
            new Claim(Consts.ClaimTypes.UserName, user.UserName ?? ""),
            new Claim(Consts.ClaimTypes.Email, user.Email ?? ""),
        ];
    }
}
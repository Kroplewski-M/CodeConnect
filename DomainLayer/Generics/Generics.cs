using System.Globalization;
using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;

namespace DomainLayer.Generics;

public static class Generics
{
    public static List<Claim> GetClaimsForUser(ApplicationUser user)
    {
        return
        [
            new Claim(Consts.ClaimTypes.FirstName, user.FirstName ?? ""),
            new Claim(Consts.ClaimTypes.LastName, user.LastName ?? ""),
            new Claim(Consts.ClaimTypes.UserName, user.UserName ?? ""),
            new Claim(Consts.ClaimTypes.Bio, user.Bio ?? ""),
            new Claim(Consts.ClaimTypes.Email, user.Email ?? ""),
            new Claim(Consts.ClaimTypes.Dob, user.DOB.ToString(CultureInfo.InvariantCulture)),
            new Claim(Consts.ClaimTypes.CreatedAt, user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
            new Claim(Consts.ClaimTypes.ProfileImg, user.ProfileImage ?? ""),
            new Claim(Consts.ClaimTypes.BackgroundImg, user.BackgroundImage ?? ""),
            new Claim(Consts.ClaimTypes.GithubLink, user.GithubLink ?? ""),
            new Claim(Consts.ClaimTypes.WebsiteLink, user.WebsiteLink ?? ""),
        ];
    }
}
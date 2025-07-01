using System.Globalization;
using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;

namespace ApplicationLayer.ExtensionClasses;

public static class ApplicationUserExtensions
{
    public static List<Claim> GetClaimsForUser(this ApplicationUser user)
    {
        string? dob = null;
        if (user.DOB != null)
            dob = user.DOB?.ToString(CultureInfo.InvariantCulture);
        return
        [
            new Claim(Consts.ClaimTypes.Id, user.Id),
            new Claim(Consts.ClaimTypes.FirstName, user.FirstName ?? ""),
            new Claim(Consts.ClaimTypes.LastName, user.LastName ?? ""),
            new Claim(Consts.ClaimTypes.UserName, user.UserName ?? ""),
            new Claim(Consts.ClaimTypes.Bio, user.Bio ?? ""),
            new Claim(Consts.ClaimTypes.Email, user.Email ?? ""),
            new Claim(Consts.ClaimTypes.Dob, dob ?? ""),
            new Claim(Consts.ClaimTypes.CreatedAt, user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
            new Claim(Consts.ClaimTypes.ProfileImg, user.ProfileImage ?? ""),
            new Claim(Consts.ClaimTypes.BackgroundImg, user.BackgroundImage ?? ""),
            new Claim(Consts.ClaimTypes.GithubLink, user.GithubLink ?? ""),
            new Claim(Consts.ClaimTypes.WebsiteLink, user.WebsiteLink ?? ""),
        ];
    }
}
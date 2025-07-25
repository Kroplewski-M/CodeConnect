using System.Globalization;
using System.Security.Claims;
using ApplicationLayer.DTO_s.User;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Helpers;

namespace ApplicationLayer.ExtensionClasses;

public static class ApplicationUserExtensions
{
    public static List<Claim> GetClaimsForUser(this ApplicationUser user)
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

    public static UserBasicDto ToUserBasicDto(this ApplicationUser? user)
    {
        if (user == null)
            return new UserBasicDto("", "", "");
        return new UserBasicDto(user.UserName ?? "", user.Bio ?? "", Helpers.GetUserImgUrl(user.ProfileImage!, Consts.ImageType.ProfileImages));
    }
}
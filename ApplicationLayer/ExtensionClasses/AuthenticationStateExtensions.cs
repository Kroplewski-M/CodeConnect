using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.ExtensionClasses;

public static class AuthenticationStateExtensions
{
    public static string GetUserInfo(this AuthenticationState? authState, string claimType)
    {
        if (authState == null || string.IsNullOrEmpty(claimType))
            return "";
        return authState.User.FindFirst(c => c.Type == claimType)?.Value ?? string.Empty;
    }
}
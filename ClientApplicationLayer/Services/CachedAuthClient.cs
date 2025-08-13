using ApplicationLayer.DTO_s.User;
using ClientApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientApplicationLayer.Services;

public class CachedAuthClient( AuthenticationStateProvider authenticationStateProvider) : ICachedAuth
{
    private static UserDetails? _cachedUser;   
    public UserDetails? GetCachedUser()
    {
        return _cachedUser;
    }

    public void ClearCacheAndNotify()
    {
        _cachedUser = null;
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
    }

    public void SetCachedUserAndNotify(UserDetails? user)
    {
        _cachedUser = user;
        ((ClientAuthStateProvider)authenticationStateProvider).NotifyStateChanged();
    }
}
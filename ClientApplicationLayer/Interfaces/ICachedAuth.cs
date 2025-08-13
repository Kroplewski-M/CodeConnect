using ApplicationLayer.DTO_s.User;

namespace ClientApplicationLayer.Interfaces;

public interface ICachedAuth
{
    public UserDetails? GetCachedUser(); 
    public void ClearCacheAndNotify();
    public void SetCachedUserAndNotify(UserDetails? user);
}
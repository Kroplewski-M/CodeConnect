using DomainLayer.Entities;

namespace ApplicationLayer.Interfaces;

public interface IUserService
{
    public Task UpdateUserDetails(EditProfileForm editProfileForm);

}
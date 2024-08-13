using ApplicationLayer.DTO_s;
using DomainLayer.Entities;

namespace ApplicationLayer.Interfaces;

public interface IUserService
{
    public Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm);

}
using ApplicationLayer.DTO_s;
using DomainLayer.Entities;

namespace ApplicationLayer.Interfaces;

public interface IUserService
{
    public Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm);
    public Task<UserDetails?> GetUserDetails(string username);
    public Task<UserInterestsDto> GetUserInterests(string username);
    public Task<ServiceResponse> UpdateUserInterests(string username,List<UserInterestsDto> userInterests);

}
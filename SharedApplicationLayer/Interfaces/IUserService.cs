using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using DomainLayer.DbEnts;
using DomainLayer.Entities;

namespace ApplicationLayer.Interfaces;

public interface IUserService
{
    public Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm, string? userId = null);
    public Task<UserDetails?> GetUserDetails(string username);
    public Task<UserInterestsDto> GetUserInterests(string username);
    public Task<ServiceResponse> UpdateUserInterests(UpdateTechInterestsDto interests);
    public Task<List<TechInterestsDto>> GetAllInterests();
}
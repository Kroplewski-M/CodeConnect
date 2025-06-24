using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using DomainLayer.DbEnts;
using DomainLayer.Entities;

namespace ApplicationLayer.Interfaces;

public interface IUserService
{
    public Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm);
    public Task<UserDetails?> GetUserDetails(string? userId = null);
    public Task<UserInterestsDto> GetUserInterests(string? userId = null);
    public Task<ServiceResponse> UpdateUserInterests(UpdateTechInterestsDto interests);
    public Task<List<TechInterestsDto>> GetAllInterests();
}
using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.Interfaces;

public interface IAuthenticateService
{
    public Task<ServiceResponse> CreateUser(RegisterFormViewModel registerForm);
    public Task<ServiceResponse> LoginUser();
    public Task<ServiceResponse> LogoutUser();
}
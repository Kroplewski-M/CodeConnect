using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.Interfaces;

public interface IAuthenticateService
{
    public Task<TokenResponse> CreateUser(RegisterFormViewModel registerForm);
    public Task<TokenResponse> LoginUser();
    public Task<ServiceResponse> LogoutUser();
}
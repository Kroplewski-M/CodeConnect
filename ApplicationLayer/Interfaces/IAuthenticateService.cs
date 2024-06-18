using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.Interfaces;

public interface IAuthenticateService
{
    public Task<AuthResponse> CreateUser(RegisterFormViewModel registerForm);
    public Task<AuthResponse> LoginUser();
}
using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.Interfaces;

public interface IAuthenticateService
{
    public Task<RegisterResponse> CreateUser(RegisterFormViewModel registerForm);
    public Task<RegisterResponse> LoginUser();
}
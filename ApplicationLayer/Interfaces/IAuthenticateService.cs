using System.Security.Claims;
using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.Interfaces;

public interface IAuthenticateService
{
    public Task<AuthResponse> CreateUser(RegisterForm registerForm);
    public Task<AuthResponse> LoginUser(LoginForm loginForm);
    public Task<AuthResponse> LogoutUser();
}
using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace ApplicationLayer.Interfaces;

public interface IAuthenticateServiceClient
{
    public UserDetails GetUserFromFromAuthState(AuthenticationState? authState);
    public Task<AuthResponse> CreateUser(RegisterForm registerForm);
    public Task<AuthResponse> LoginUser(LoginForm loginForm);
    public Task<AuthResponse> LogoutUser();
}
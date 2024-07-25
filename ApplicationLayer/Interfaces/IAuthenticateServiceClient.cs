using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Auth;

namespace ApplicationLayer.Interfaces;

public interface IAuthenticateServiceClient
{
    public event Action? OnChange;
    public Task<AuthResponse> CreateUser(RegisterForm registerForm);
    public Task<AuthResponse> LoginUser(LoginForm loginForm);
    public Task<AuthResponse> LogoutUser();
    public UserDetails GetUserDetails();
    public Task InitializeAsync();
}
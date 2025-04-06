using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.ExtensionClasses;
using Blazored.LocalStorage;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using DomainLayer.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace ApplicationLayer.ClientServices;

public class ClientAuthStateProvider(HttpClient httpClient,
    ILocalStorageService localStorageService, NavigationManager navigationManager,
    NotificationsService notificationsService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
 
        var token = await localStorageService.GetItemAsync<string>(Consts.Tokens.AuthToken);
        Console.WriteLine("Token retrieved: " + token);
        if(string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal());
        var isValid = await IsTokenValid(token);
        Console.WriteLine("Token is valid: " + isValid);

        return isValid ? CreateAuthenticationStateFromToken(token) 
                       : new AuthenticationState(new ClaimsPrincipal());
    }

    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        Console.WriteLine("NotifyStateChanged");
    }
    private async Task<bool> IsTokenValid(string? token)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/ValidateToken", token);
        return response.IsSuccessStatusCode;
    }
    private AuthenticationState CreateAuthenticationStateFromToken(string? token)
    {
        var authState = new AuthenticationState(new ClaimsPrincipal(DecodeToken(token)));
        if(navigationManager.Uri.EndsWith("/"))
            navigationManager.NavigateTo("/MyFeed");
        return authState;
    }
    private ClaimsPrincipal DecodeToken(string? token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);

        var claims = new List<Claim>();
        foreach (var claim in jwtSecurityToken.Claims)
        {
            claims.Add(new Claim(claim.Type, claim.Value));
        }

        var identity = new ClaimsIdentity(claims, Consts.Tokens.AuthType);
        return new ClaimsPrincipal(identity);
    }
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/RegisterUser", registerForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken, authResponse.Token);

                NotifyStateChanged();
                return authResponse;
            }
        }
        return new AuthResponse(false, "","", authResponse?.Message ?? "An error occured please try again later");
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/Authentication/LoginUser", loginForm);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (response.IsSuccessStatusCode)
        {
            if (authResponse != null && authResponse.Flag)
            {
                await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, authResponse.Token);
                await localStorageService.SetItemAsync(Consts.Tokens.RefreshToken, authResponse.RefreshToken);
                NotifyStateChanged();
                return authResponse;
            }
        }
        return new AuthResponse(false, "","", authResponse?.Message ?? "Error occured during login please try again later");
    }

    public UserDetails GetUserFromFromAuthState(AuthenticationState? authState)
    {
        var dob = authState.GetUserInfo(Consts.ClaimTypes.Dob).Trim() ?? null;
        var createdAt = authState.GetUserInfo(Consts.ClaimTypes.CreatedAt).Trim() ?? null;
        string format = "MM/dd/yyyy";
        var profileImg = authState.GetUserInfo(Consts.ClaimTypes.ProfileImg);
        var backgroundImg = authState.GetUserInfo(Consts.ClaimTypes.BackgroundImg);

        profileImg = string.IsNullOrEmpty(profileImg) ? Consts.ProfileDefaults.ProfileImg 
                     : Helpers.GetAzureImgUrl(Consts.ImageType.ProfileImages, profileImg);
        backgroundImg = string.IsNullOrEmpty(backgroundImg) ? Consts.ProfileDefaults.BackgroundImg 
                        : Helpers.GetAzureImgUrl(Consts.ImageType.BackgroundImages, backgroundImg);

        return new UserDetails(
            FirstName: authState.GetUserInfo(Consts.ClaimTypes.FirstName),
            LastName: authState.GetUserInfo(Consts.ClaimTypes.LastName),
            Email: authState.GetUserInfo(Consts.ClaimTypes.Email),
            UserName: authState.GetUserInfo(Consts.ClaimTypes.UserName),
            ProfileImg: profileImg,
            BackgroundImg: backgroundImg,
            GithubLink: authState.GetUserInfo(Consts.ClaimTypes.GithubLink),
            WebsiteLink: authState.GetUserInfo(Consts.ClaimTypes.WebsiteLink),
            Dob: DateOnly.ParseExact(dob ?? "", format, CultureInfo.InvariantCulture), 
            CreatedAt:DateOnly.ParseExact(createdAt ?? "", format, CultureInfo.InvariantCulture),
            Bio:authState.GetUserInfo(Consts.ClaimTypes.Bio)
            );
    }
    public async Task<AuthResponse> LogoutUser()
    {
        await localStorageService.RemoveItemAsync(Consts.Tokens.AuthToken);
        await localStorageService.RemoveItemAsync(Consts.Tokens.RefreshToken);
        NotifyStateChanged();
        navigationManager.NavigateTo("/");
        notificationsService.PushNotification(new Notification("Logged out successfully",NotificationType.Success));
        return new AuthResponse(true, "","", "Logged out successfully");
    }
    public async Task<ServiceResponse> UpdateUserDetails(EditProfileForm editProfileForm)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/EditUserDetails", editProfileForm);
        var newToken = await response.Content.ReadFromJsonAsync<TokenResponse>();
        await localStorageService.SetItemAsync(Consts.Tokens.AuthToken, newToken?.Key);
        NotifyStateChanged();
        return new ServiceResponse(true, "Updated Details Successfully");
    }

    public async Task<UserDetails?> GetUserDetails(string username)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/GetUserDetails", username);
        if(response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<UserDetails?>();
        navigationManager.NavigateTo("UserNotFound");
        return null;
    }

    public async Task<UserInterestsDto> GetUserInterests(string username)
    {
        var response = await httpClient.PostAsJsonAsync("/api/User/GetUserInterests", username);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UserInterestsDto>();
            if(result != null)
                return result;
            return new UserInterestsDto(false, "failed to fetch interests", null);
        }
        return new UserInterestsDto(false, "failed to fetch interests", null);
    }

    public async Task<ServiceResponse> UpdateUserInterests(string? username, List<TechInterestsDto> interests)
    {
        var response = await httpClient.PutAsJsonAsync("api/User/UpdateUserInterests",interests);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        if (result != null)
            return result;
        return new ServiceResponse(false, "An error occured while updating the interests");
    }

    public async Task<List<TechInterestsDto>> GetAllInterests()
    {
        var response = await httpClient.GetFromJsonAsync<List<TechInterestsDto>>("api/User/GetAllInterests");
        return response ?? new List<TechInterestsDto>();
    }
}

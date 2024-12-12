using System.Globalization;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using FluentValidation;
using FluentValidation.Results;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.APIServices;

public class AuthenticateService(UserManager<ApplicationUser>userManager,
    TokenService tokenGenerationService,  ApplicationDbContext context) : IAuthenticateService
{
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm)
    {
        var user = new ApplicationUser
        {
            UserName = registerForm.UserName,
            FirstName = registerForm.FirstName,
            LastName = registerForm.LastName,
            Email = registerForm.Email,
            DOB = registerForm.Dob,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
        };
        var result = await userManager.CreateAsync(user, registerForm.Password);
        if (result.Succeeded)
        {
            var userClaims = GetClaimsForUser(user);
            var createdRefresh = await SaveRefreshToken(userClaims, user.Id);
            if (!string.IsNullOrWhiteSpace(createdRefresh))
                return new AuthResponse(false,"","","Error creating refresh token");
            return GenerateAuthResponse(userClaims,createdRefresh);
        }
        string error = result.Errors.Select(x => x.Description).FirstOrDefault() ?? "";
        return new AuthResponse(false,"","",error);
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm)
    {
        ApplicationUser? user = null;
        var validator = new InlineValidator<string>();
        validator.RuleFor(x=> x).EmailAddress();
        ValidationResult isEmail = await validator.ValidateAsync(loginForm.Email);
        if (isEmail.IsValid)
        {
            user = await userManager.FindByEmailAsync(loginForm.Email);
        }
        else
        {
            user = await userManager.FindByNameAsync(loginForm.Email);
        }
        if (user != null)
        {
            var correctPassword = await userManager.CheckPasswordAsync(user, loginForm.Password);
            if (correctPassword)
            {
                var userClaims = GetClaimsForUser(user);
                var createdRefresh = await SaveRefreshToken(userClaims, user.Id);
                if (string.IsNullOrWhiteSpace(createdRefresh))
                    return new AuthResponse(false,"","","Error creating refresh token");
                return GenerateAuthResponse(userClaims,createdRefresh);
            }
        }
        return new AuthResponse(false, "","","Incorrect Email or Password");
    }

    public async Task<string> SaveRefreshToken(List<Claim> userClaims,string userId)
    {
        var existingToken = context.RefreshUserAuths.FirstOrDefault(x=> x.UserId == userId);
        if (existingToken != null)
            context.RefreshUserAuths.Remove(existingToken);
        //Save refresh token in DB to re authenticate user
        var refreshExpiresAt = DateTime.UtcNow.AddMinutes(Constants.Tokens.RefreshTokenMins);
        var refreshToken = tokenGenerationService.GenerateJwtToken(userClaims,refreshExpiresAt);
        if(string.IsNullOrWhiteSpace(refreshToken))
            return "";
        context.RefreshUserAuths.Add(new RefreshUserAuth { RefreshToken = refreshToken, UserId = userId});
        await context.SaveChangesAsync();
        return refreshToken;
    }
    public Task<AuthResponse> LogoutUser()
    {
        throw new NotImplementedException();
    }

    public List<Claim> GetClaimsForUser(ApplicationUser user)
    {
        return
        [
            new Claim(Constants.ClaimTypes.FirstName, user.FirstName ?? ""),
            new Claim(Constants.ClaimTypes.LastName, user.LastName ?? ""),
            new Claim(Constants.ClaimTypes.UserName, user.UserName ?? ""),
            new Claim(Constants.ClaimTypes.Bio, user.Bio ?? ""),
            new Claim(Constants.ClaimTypes.Email, user.Email ?? ""),
            new Claim(Constants.ClaimTypes.Dob, user.DOB.ToString(CultureInfo.InvariantCulture)),
            new Claim(Constants.ClaimTypes.CreatedAt, user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
            new Claim(Constants.ClaimTypes.ProfileImg, user.ProfileImageUrl ?? ""),
            new Claim(Constants.ClaimTypes.BackgroundImg, user.BackgroundImageUrl ?? ""),
            new Claim(Constants.ClaimTypes.GithubLink, user.GithubLink ?? ""),
            new Claim(Constants.ClaimTypes.WebsiteLink, user.WebsiteLink ?? ""),
        ];
    }

    private AuthResponse GenerateAuthResponse(List<Claim> userClaims,string refreshToken)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(Constants.Tokens.AuthTokenMins);
        var token = tokenGenerationService.GenerateJwtToken(userClaims,expiresAt);
        
        return new AuthResponse(true, token, refreshToken,"Auth successful");
    }
}
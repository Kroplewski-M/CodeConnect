using System.Globalization;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using FluentValidation;
using FluentValidation.Results;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;

namespace ApplicationLayer.APIServices;

public class AuthenticateService(UserManager<ApplicationUser>userManager,
    ITokenService tokenGenerationService,  ApplicationDbContext context) : IAuthenticateService
{
    public async Task<AuthResponse> CreateUser(RegisterForm registerForm, string deviceId)
    {
        if(!Guid.TryParse(deviceId, out Guid deviceGuid))
            return new AuthResponse(false,"","","Invalid device id");
        RegisterFormValidator registerFormValidator = new RegisterFormValidator();
        var validate = await registerFormValidator.ValidateAsync(registerForm);
        if (!validate.IsValid)
            return new AuthResponse(false,"","", "Error creating user");
        var user = new ApplicationUser
        {
            UserName = registerForm.UserName,
            FirstName = registerForm.FirstName,
            LastName = registerForm.LastName,
            Email = registerForm.Email,
            DOB = registerForm.Dob,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        var result = await userManager.CreateAsync(user, registerForm.Password);
        if (result.Succeeded)
        {
            var userClaims = user.GetJwtClaimsForUser();
            var createdRefresh = await SaveRefreshToken(userClaims, user.Id,deviceGuid);
            if (string.IsNullOrWhiteSpace(createdRefresh))
                return new AuthResponse(false,"","","Error creating refresh token");
            return GenerateAuthResponse(userClaims,createdRefresh);
        }
        string error = result.Errors.Select(x => x.Description).FirstOrDefault() ?? "";
        return new AuthResponse(false,"","",error);
    }

    public async Task<AuthResponse> LoginUser(LoginForm loginForm,string deviceId)
    { 
        if(!Guid.TryParse(deviceId, out Guid deviceGuid))
            return new AuthResponse(false,"","","Invalid device id");
        var validate = new LoginFormValidator();
        var validateResult = await validate.ValidateAsync(loginForm);
        if (!validateResult.IsValid)
            return new AuthResponse(false, "", "", "Login form is invalid");
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
                var userClaims = user.GetJwtClaimsForUser();
                var createdRefresh = await SaveRefreshToken(userClaims, user.Id,deviceGuid);
                if (string.IsNullOrWhiteSpace(createdRefresh))
                    return new AuthResponse(false,"","","Error creating refresh token");
                return GenerateAuthResponse(userClaims,createdRefresh);
            }
        }
        return new AuthResponse(false, "","","Incorrect Email or Password");
    }

    protected virtual async Task<string> SaveRefreshToken(List<Claim> userClaims,string userId, Guid deviceId)
    {
        var existingToken = context.RefreshUserAuths
            .FirstOrDefault(x=> x.UserId == userId && x.Expires > DateTime.UtcNow && x.DeviceId == deviceId);

        //Save refresh token in DB to re authenticate user
        var refreshExpiresAt = DateTime.UtcNow.AddMinutes(Consts.Tokens.RefreshTokenMins);
        var refreshToken = tokenGenerationService.GenerateJwtToken(userClaims,refreshExpiresAt, Consts.TokenType.Refresh);
        if(string.IsNullOrWhiteSpace(refreshToken))
            return "";
        if (existingToken != null)
        {
            existingToken.RefreshToken = refreshToken;
            existingToken.Expires = refreshExpiresAt;
            await context.SaveChangesAsync();
            return refreshToken;
        }
        context.RefreshUserAuths.Add(new RefreshUserAuth 
        { 
            RefreshToken = refreshToken,
            UserId = userId, 
            DeviceId = deviceId, 
            Expires = refreshExpiresAt
        });
        await context.SaveChangesAsync();
        return refreshToken;
    }

    public Task<AuthResponse> LogoutUser()
    {
        throw new NotImplementedException();
    }
    private AuthResponse GenerateAuthResponse(List<Claim> userClaims,string refreshToken)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(Consts.Tokens.AuthTokenMins);
        var token = tokenGenerationService.GenerateJwtToken(userClaims,expiresAt);
        
        return new AuthResponse(true, token, refreshToken,"Auth successful");
    }
}
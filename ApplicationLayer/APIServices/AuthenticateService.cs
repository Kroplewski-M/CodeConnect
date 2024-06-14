using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationLayer.APIServices;

public class AuthenticateService(UserManager<ApplicationUser>userManager) : IAuthenticateService
{
    public async Task<TokenResponse> CreateUser(RegisterFormViewModel registerForm)
    {
        var user = new ApplicationUser
        {
            UserName = registerForm.Email,
            FirstName = registerForm.FirstName,
            LastName = registerForm.LastName,
            Email = registerForm.Email,
            DOB = registerForm.DOB,
            CreatedAt = DateTime.Now,
        };
        var result = await userManager.CreateAsync(user, registerForm.Password);
        if (result.Succeeded)
        {
            var claims = new List<Claim>
            {
                new Claim("FirstName", registerForm.FirstName),
                new Claim("LastName", registerForm.LastName),
                new Claim(ClaimTypes.Email, registerForm.Email),
                new Claim("DOB", registerForm.DOB.ToString(CultureInfo.InvariantCulture)),
                new Claim("CreatedAt", user.CreatedAt.ToString(CultureInfo.InvariantCulture)),
            };
            var expiresAt = DateTime.UtcNow.AddMinutes(60);
            var token = CreateToken(claims, expiresAt);
            return new TokenResponse(token,expiresAt);
        }
        else
        {
            return new TokenResponse("",DateTime.Now);
        }
    }

    public async Task<TokenResponse> LoginUser()
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResponse> LogoutUser()
    {
        throw new NotImplementedException();
    }
    public string CreateToken(IEnumerable<Claim> claims, DateTime expireAt)
    {
        var secretKey = Encoding.ASCII.GetBytes( "NHr9oh7N6SDApfQJA0+pBUJp4DhHqU3MZ1I8kFtA/KE=");
        //Generate the JWT
        var JWT = new JwtSecurityToken(claims: claims, notBefore: DateTime.UtcNow, expires: expireAt,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature));
        
        return new JwtSecurityTokenHandler().WriteToken(JWT);
    }
}
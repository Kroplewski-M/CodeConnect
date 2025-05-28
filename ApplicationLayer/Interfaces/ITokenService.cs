using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;

namespace ApplicationLayer.Interfaces;

public interface ITokenService
{
    public string? GenerateJwtToken(IEnumerable<Claim> claims, DateTime expireAt);
    public ClaimsPrincipalResponse ValidateToken(string token);
    public Task<AuthResponse> RefreshUserTokens(string userName, string refreshToken);

}
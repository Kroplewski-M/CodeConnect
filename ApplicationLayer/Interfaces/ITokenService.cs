using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using DomainLayer.Constants;

namespace ApplicationLayer.Interfaces;

public interface ITokenService
{
    public string? GenerateJwtToken(IEnumerable<Claim> claims, DateTime expireAt, Consts.TokenType tokenType = Consts.TokenType.Access);
    public ClaimsPrincipalResponse ValidateToken(string token);
    public Task<AuthResponse> RefreshUserTokens(string refreshToken);

}
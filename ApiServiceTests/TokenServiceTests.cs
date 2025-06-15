using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApplicationLayer.APIServices;
using DomainLayer.Constants;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace ApiServiceTests;

[Collection("DatabaseCollection")]
public class TokenServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly TokenService _tokenService;
    public TokenServiceTests(DatabaseFixture databaseFixture)
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!,
            null!, null!, null!);
        _context = databaseFixture.Context;
        var jwtSettings = Options.Create(new JwtSettings()
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "super_secret_key_super_secret_key",
        });
        _tokenService = new TokenService(jwtSettings, _context, _userManager.Object);
    }
    [Fact]
    public void GenerateJwtToken_WithValidClaims_ReturnsValidToken()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "testuser"),
        };
        var expireAt = DateTime.UtcNow.AddMinutes(30);

        // Act
        var token = _tokenService.GenerateJwtToken(claims, expireAt);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Equal("TestAudience", jwtToken.Audiences.First());
    }
    [Fact]
    public void GenerateJwtToken_WithEmptyClaims_ReturnsEmptyString()
    {
        // Arrange
        var expireAt = DateTime.UtcNow.AddMinutes(30);

        // Act
        var token = _tokenService.GenerateJwtToken([], expireAt);

        // Assert
        Assert.NotNull(token);
        Assert.Empty(token);
    }
    [Fact]
    public void ValidateToken_WithValidToken_ReturnsAuthenticated()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };
        var expireAt = DateTime.UtcNow.AddMinutes(30);
        var token = _tokenService.GenerateJwtToken(claims, expireAt);

        // Act
        var result = _tokenService.ValidateToken(token ?? "");

        // Assert
        Assert.True(result.Flag);
        Assert.NotNull(result.ClaimsPrincipal);
        Assert.True(result.ClaimsPrincipal.Identity?.IsAuthenticated);
    }
    [Fact]
    public void ValidateToken_WithExpiredToken_ReturnsNotAuthenticated()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };
        var expireAt = DateTime.UtcNow.AddMinutes(-30); // Already expired
        var token = _tokenService.GenerateJwtToken(claims, expireAt);

        // Act
        var result = _tokenService.ValidateToken(token ?? "");

        // Assert
        Assert.False(result.Flag);
    }
    [Fact]
    public void ValidateToken_WithWrongSignature_ReturnsNotAuthenticated()
    {
        // Arrange
        var differentSettings = Options.Create(new JwtSettings
        {
            Key = "AnotherVerySecureDifferentKey123456!",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        });
        var differentService = new TokenService(differentSettings, null!, null!);

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };
        var expireAt = DateTime.UtcNow.AddMinutes(30);
        var token = differentService.GenerateJwtToken(claims, expireAt);

        // Act
        var result = _tokenService.ValidateToken(token ?? ""); 

        // Assert
        Assert.False(result.Flag);
    }
    [Fact]
    public void ValidateToken_WithGarbageToken_ReturnsNotAuthenticated()
    {
        // Arrange
        var garbageToken = "ThisIsNotARealJWTToken";

        // Act
        var result = _tokenService.ValidateToken(garbageToken);

        // Assert
        Assert.False(result.Flag);
    }
    [Fact]
    public void ValidateToken_WithEmptyToken_ReturnsNotAuthenticated()
    {
        // Arrange
        var garbageToken = "";

        // Act
        var result = _tokenService.ValidateToken(garbageToken);

        // Assert
        Assert.False(result.Flag);
    }
    [Fact]
    public async Task RefreshUserTokens_UserNotFound_ReturnsError()
    {
        // Arrange
        _userManager.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _tokenService.RefreshUserTokens( "sometoken",Guid.NewGuid().ToString());

        // Assert
        Assert.False(result.Flag);
        Assert.NotNull(result.Token);
        Assert.Empty(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.Empty(result.RefreshToken);
    }
    [Fact]
    public async Task RefreshUserTokens_RefreshTokenMismatch_ReturnsError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user123", UserName = "testuser" };
        _userManager.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
        _context.RefreshUserAuths.Add(new RefreshUserAuth { UserId = "user123", RefreshToken = "token", DeviceId = Guid.NewGuid()});
        await _context.SaveChangesAsync();
        // Act
        var result = await _tokenService.RefreshUserTokens( "wrongtoken",Guid.NewGuid().ToString());

        // Assert
        Assert.False(result.Flag);
        Assert.NotNull(result.Token);
        Assert.Empty(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.Empty(result.RefreshToken);
    }
    [Fact]
    public async Task RefreshUserTokens_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user123", UserName = "testuser" };
        _userManager.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
        var token = _tokenService.GenerateJwtToken(new List<Claim>
        {
            new (Consts.ClaimTypes.UserName, "testuser"),
            new (Consts.Tokens.TokenType, nameof(Consts.TokenType.Refresh))
        }, DateTime.UtcNow.AddMinutes(30));
        var deviceId = Guid.NewGuid();
        var refreshAuth = new RefreshUserAuth { UserId = "user123", RefreshToken = token, DeviceId = deviceId, Expires = DateTime.UtcNow.AddMinutes(30)};

        _context.RefreshUserAuths.Add(refreshAuth);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenService.RefreshUserTokens(token,deviceId.ToString());

        // Assert
        Assert.True(result.Flag);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
    }
}
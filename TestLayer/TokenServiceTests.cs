using System.Globalization;
using System.Security.Claims;
using ApplicationLayer.APIServices;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;
using Moq;

namespace TestLayer;

public class TokenServiceTests
{
    // private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
    // [Fact]
    // public void CreateTokenBasedOnUserSettings_ShouldNotBeNull()
    // {
    //     //Arrange
    //     // _mockJwtSettings.Setup(m => m.Value).Returns(new JwtSettings
    //     // {
    //     //     Key = "abcdefghijklmnopqrstuvwx01234567",
    //     //     Issuer = "issuer",
    //     //     Audience = "audience"
    //     // });
    //     // var tokenService = new TokenService(_mockJwtSettings.Object,);
    //     // var claims = new List<Claim>
    //     // {
    //     //     new Claim(ClaimTypes.Name, "John"),
    //     //     new Claim(ClaimTypes.Email, "John@gmail.com"),
    //     //     new Claim("DOB", DateTime.Now.ToString(CultureInfo.InvariantCulture))
    //     // };
    //     // var expiresAt = DateTime.Now.AddHours(1);
    //     // //Act
    //     // var result = tokenService.GenerateJwtToken(claims, expiresAt);
    //     // //Assert
    //     // Assert.NotNull(result);
    // }
    // [Fact]
    // public void VerifyToken_ShouldReturnTrue()
    // {
    //     //Arrange
    //     // _mockJwtSettings.Setup(m => m.Value).Returns(new JwtSettings
    //     // {
    //     //     Key = "abcdefghijklmnopqrstuvwx01234567",
    //     //     Issuer = "issuer",
    //     //     Audience = "audience"
    //     // });
    //     // var tokenService = new TokenService(_mockJwtSettings.Object);
    //     // var claims = new List<Claim>
    //     // {
    //     //     new Claim(ClaimTypes.Name, "John"),
    //     //     new Claim(ClaimTypes.Email, "John@gmail.com"),
    //     //     new Claim("DOB", DateTime.Now.ToString(CultureInfo.InvariantCulture))
    //     // };
    //     // var expiresAt = DateTime.Now.AddHours(1);
    //     // var result = tokenService.GenerateJwtToken(claims, expiresAt);
    //     // //Act
    //     // var tokenResponse = tokenService.ValidateToken(result ?? "");
    //     // //Assert
    //     // Assert.True(tokenResponse.ClaimsPrincipal.Claims.Any());
    // }
    // [Fact]
    // public void VerifyToken_ShouldReturnFalse()
    // {
    //     //Arrange
    //     // _mockJwtSettings.Setup(m => m.Value).Returns(new JwtSettings
    //     // {
    //     //     Key = "abcdefghijklmnopqrstuvwx01234567",
    //     //     Issuer = "issuer",
    //     // });
    //     // var tokenService = new TokenService(_mockJwtSettings.Object);
    //     // var claims = new List<Claim>
    //     // {
    //     //     new Claim(ClaimTypes.Name, "John"),
    //     //     new Claim(ClaimTypes.Email, "John@gmail.com"),
    //     //     new Claim("DOB", DateTime.Now.ToString(CultureInfo.InvariantCulture))
    //     // };
    //     // var expiresAt = DateTime.Now.AddHours(1);
    //     // var result = tokenService.GenerateJwtToken(claims, expiresAt);
    //     // //Act
    //     // var tokenResponse = tokenService.ValidateToken(result ?? "");
    //     // //Assert
    //     // Assert.False(tokenResponse.Flag);
    // }
    //
    // [Fact]
    // public void CreateNewTokenFromRefreshToken_ShouldReturnNewToken()
    // {
    //     //Arrange
    //     // _mockJwtSettings.Setup(m => m.Value).Returns(new JwtSettings
    //     // {
    //     //     Key = "abcdefghijklmnopqrstuvwx01234567",
    //     //     Issuer = "issuer",
    //     //     Audience = "audience"
    //     // });
    //     // var tokenService = new TokenService(_mockJwtSettings.Object);
    //     // var claims = new List<Claim>
    //     // {
    //     //     new Claim(ClaimTypes.Name, "John"),
    //     //     new Claim(ClaimTypes.Email, "John@gmail.com"),
    //     //     new Claim("DOB", DateTime.Now.ToString(CultureInfo.InvariantCulture))
    //     // };
    //     // var expiresAt = DateTime.Now.AddHours(1);
    //     // var result = tokenService.GenerateJwtToken(claims, expiresAt);
    //     //Act
    //     //var tokenResponse = tokenService.RefreshToken(result ?? "",claims.AsEnumerable());
    //     //Assert
    //     //Assert.NotEmpty(tokenResponse.Key ?? "");
    // }
}
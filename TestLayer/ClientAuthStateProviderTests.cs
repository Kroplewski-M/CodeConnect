using System.Globalization;
using System.Security.Claims;
using ApplicationLayer.APIServices;
using DomainLayer.Entities.APIClasses;
using Microsoft.Extensions.Options;
using Moq;

namespace TestLayer;

public class ClientAuthStateProviderTests
{
    private readonly Mock<IOptions<JwtSettings>> mockJwtSettings = new Mock<IOptions<JwtSettings>>();
    [Fact]
    public void CreateTokenBasedOnUserSettings_ShouldNotBeNull()
    {
        //Arrange
        mockJwtSettings.Setup(m => m.Value).Returns(new JwtSettings
        {
            Key = "abcdefghijklmnopqrstuvwx01234567",
            Issuer = "issuer",
            Audience = "audience"
        });
        var tokenService = new TokenService(mockJwtSettings.Object);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "John"),
            new Claim(ClaimTypes.Email, "John@gmail.com"),
            new Claim("DOB", DateTime.Now.ToString(CultureInfo.InvariantCulture))
        };
        var expiresAt = DateTime.Now.AddHours(1);
        //Act
        var result = tokenService.GenerateJwtToken(claims, expiresAt);
        //Assert
        Assert.NotNull(result);
    }
}
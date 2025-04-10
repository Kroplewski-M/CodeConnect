using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ApiServiceTests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IAuthenticateService> _authServiceMock;
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, 
            null!, null!, null!, null!, null!, null!, null!, null!
        );
        _tokenServiceMock = new Mock<ITokenService>();
        _authServiceMock = new Mock<IAuthenticateService>();
    }

    [Fact]
    public async Task CreateUser_ShouldReturnSuccessResponse_WhenRegistrationIsValid()
    {
        //Arrange
        var context = GetInMemoryDbContext();
        var registerForm = new RegisterForm
        {
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Dob = DateOnly.Parse("1990-01-01"),
            Password = "TestPassword123!"
        };
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        _tokenServiceMock.Setup(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()))
            .Returns("dummy-refresh-token");
        var service = new AuthenticateService(_userManagerMock.Object, _tokenServiceMock.Object, context);
        //Act
        var result = await service.CreateUser(registerForm);
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        Assert.Equal("dummy-refresh-token", result.RefreshToken);
        Assert.Equal("dummy-refresh-token", result.Token);

    }
}
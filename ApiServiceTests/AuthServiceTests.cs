using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.APIClasses;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace ApiServiceTests;


public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IAuthenticateService> _authServiceMock;
    private class TestableAuth(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ApplicationDbContext context)
        : AuthenticateService(userManager, tokenService, context)
    {
        private readonly ITokenService _tokenService = tokenService;
        public int SaveRefreshCount { get; private set; } = 0;
        protected override  Task<string> SaveRefreshToken(List<Claim> claims, string userId)
        {
            SaveRefreshCount++;
            return Task.FromResult(_tokenService.GenerateJwtToken(new List<Claim> { new Claim(ClaimTypes.Name, userId) }, DateTime.UtcNow))!;
        }
    }
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
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password), Times.Once);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Exactly(2));

    }
    [Fact]
    public async Task CreateUser_NoUsername_ShouldReturnBadRequestResponse()
    {
        //Arrange
        var context = GetInMemoryDbContext();
        var registerForm = new RegisterForm
        {
            UserName = "",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Dob = DateOnly.Parse("1990-01-01"),
            Password = "TestPassword123!"
        };
        
        var service = new AuthenticateService(_userManagerMock.Object, _tokenServiceMock.Object, context);
        //Act
        var result = await service.CreateUser(registerForm);
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password), Times.Never);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Never);
    }
    [Fact]
    public async Task CreateUser_NoFirstName_ShouldReturnBadRequestResponse()
    {
        //Arrange
        var context = GetInMemoryDbContext();
        var registerForm = new RegisterForm
        {
            UserName = "testUsername",
            FirstName = "",
            LastName = "User",
            Email = "test@example.com",
            Dob = DateOnly.Parse("1990-01-01"),
            Password = "TestPassword123!"
        };
        
        var service = new AuthenticateService(_userManagerMock.Object, _tokenServiceMock.Object, context);
        //Act
        var result = await service.CreateUser(registerForm);
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password), Times.Never);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Never);
    }
    [Fact]
    public async Task CreateUser_NoLastName_ShouldReturnBadRequestResponse()
    {
        //Arrange
        var context = GetInMemoryDbContext();
        var registerForm = new RegisterForm
        {
            UserName = "testUsername",
            FirstName = "User",
            LastName = "",
            Email = "test@example.com",
            Dob = DateOnly.Parse("1990-01-01"),
            Password = "TestPassword123!"
        };
        
        var service = new AuthenticateService(_userManagerMock.Object, _tokenServiceMock.Object, context);
        //Act
        var result = await service.CreateUser(registerForm);
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password), Times.Never);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Never);
    }
    [Fact]
    public async Task CreateUser_NoEmail_ShouldReturnBadRequestResponse()
    {
        //Arrange
        var context = GetInMemoryDbContext();
        var registerForm = new RegisterForm
        {
            UserName = "testUsername",
            FirstName = "User",
            LastName = "User",
            Email = "",
            Dob = DateOnly.Parse("1990-01-01"),
            Password = "TestPassword123!"
        };
        
        var service = new AuthenticateService(_userManagerMock.Object, _tokenServiceMock.Object, context);
        //Act
        var result = await service.CreateUser(registerForm);
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password), Times.Never);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Never);
    }
    [Fact]
    public async Task CreateUser_NoPassword_ShouldReturnBadRequestResponse()
    {
        //Arrange
        var context = GetInMemoryDbContext();
        var registerForm = new RegisterForm
        {
            UserName = "testUsername",
            FirstName = "User",
            LastName = "User",
            Email = "test@example.com",
            Dob = DateOnly.Parse("1990-01-01"),
            Password = ""
        };
        
        var service = new AuthenticateService(_userManagerMock.Object, _tokenServiceMock.Object, context);
        //Act
        var result = await service.CreateUser(registerForm);
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password), Times.Never);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Never);
    }
    [Fact]
    public async Task CreateUser_PasswordUnderMinLength_ShouldReturnBadRequestResponse()
    {
        //Arrange
        var context = GetInMemoryDbContext();
        var registerForm = new RegisterForm
        {
            UserName = "testUsername",
            FirstName = "User",
            LastName = "User",
            Email = "test@example.com",
            Dob = DateOnly.Parse("1990-01-01"),
            Password = "thisIsN"
        };
        
        var service = new AuthenticateService(_userManagerMock.Object, _tokenServiceMock.Object, context);
        //Act
        var result = await service.CreateUser(registerForm);
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerForm.Password), Times.Never);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task LoginUser_WithEmail_ShouldCallFindByEmail_ShouldReturnOkResponse()
    {
        //Arrange 
        var context = GetInMemoryDbContext();
        var loginForm = new LoginForm()
        {
            Email = "testUsername@gmail.com",
            Password = "TestPassword123!",
        };
        var service = new TestableAuth(_userManagerMock.Object, _tokenServiceMock.Object, context);
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser() {Email = loginForm.Email});
        _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(true);
        _tokenServiceMock.Setup(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>())).Returns("token");
        //Act
        var result = await service.LoginUser(loginForm);
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Never);
        Assert.Equal(1, service.SaveRefreshCount);
        _tokenServiceMock.Verify(ts => ts.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Exactly(2));
    }

    [Fact]
    public async Task LoginUser_WithUsername_ShouldCallFindByUsername_ShouldReturnOkResponse()
    {
        //Arrange  
        var context = GetInMemoryDbContext();
        var loginForm = new LoginForm()
        {
            Email = "testUsername",
            Password = "TestPassword123!"
        };
        var service = new TestableAuth(_userManagerMock.Object, _tokenServiceMock.Object, context);
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "testUsername"});
        _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()))
            .Returns("token");
        //Act  
        var result = await service.LoginUser(loginForm);
        
        //Assert
        Assert.NotNull(result);
        Assert.True(result.Flag);
        Assert.Equal(1, service.SaveRefreshCount);
        _userManagerMock.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never); 
        _tokenServiceMock.Verify(x => x.GenerateJwtToken(It.IsAny<List<Claim>>(),It.IsAny<DateTime>()), Times.Exactly(2));
    }

    [Fact]
    public async Task LoginUser_NoPassword_ShouldReturnBadRequestResponse()
    {
        // Arrange 
        var context = GetInMemoryDbContext();
        var loginForm = new LoginForm()
        {
            Email = "testUsername",
            Password = ""
        };
        
        //Act  
        var service = new TestableAuth(_userManagerMock.Object, _tokenServiceMock.Object, context);
        var result = await service.LoginUser(loginForm);
        //Assert
        Assert.NotNull(result); 
        Assert.False(result.Flag);
        _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _tokenServiceMock.Verify(x => x.GenerateJwtToken(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task LoginUser_NoEmail_ShouldReturnBadRequestResponse()
    { 
        //Arrange
        var context = GetInMemoryDbContext();
        var loginForm = new LoginForm()
        {
            Email = "",
            Password = "Password123!"
        }; 
        
        //Act 
        var service = new TestableAuth(_userManagerMock.Object, _tokenServiceMock.Object, context);
        var result = await service.LoginUser(loginForm);
        
        //Assert
        Assert.NotNull(result);
        Assert.False(result.Flag); 
        _userManagerMock.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }
}

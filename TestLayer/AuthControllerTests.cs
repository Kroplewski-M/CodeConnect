using System.ClientModel.Primitives;
using System.Net;
using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.AuthenticationEndpoint;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticateService> _authenticateServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthenticationController _authController;
    
    
    public AuthControllerTests()
    {
        _authenticateServiceMock = new Mock<IAuthenticateService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _authController = new AuthenticationController(_authenticateServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterUser_ShouldReturnOk()
    {
        // Arrange
        var registerForm = new RegisterForm() { FirstName = "Test",LastName = "Test",UserName = "testuser", Email = "test@example.com", Password = "password", Dob = DateOnly.FromDateTime(DateTime.Now) };
        var expectedResult = new AuthResponse( true, "", "", "User registered successfully");
        _authenticateServiceMock.Setup(x => x.CreateUser(registerForm)).ReturnsAsync(expectedResult);

        // Act
        var result = await _authController.RegisterUser(registerForm);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.CreateUser(registerForm), Times.Once);
    }
    [Fact]
    public async Task RegisterUser_ShouldReturnFalse()
    {
        // Arrange
        var registerForm = new RegisterForm() { UserName = "", Email = "", Password = "", Dob = DateOnly.FromDateTime(DateTime.Now) };
        var expectedResult = new AuthResponse( false, "", "", "Invalid Register Form");
        _authenticateServiceMock.Setup(x => x.CreateUser(registerForm)).ReturnsAsync(expectedResult);

        // Act
        var result = await _authController.RegisterUser(registerForm);
        var badResult = result as BadRequestObjectResult;

        // Assert
        Assert.NotNull(badResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.CreateUser(registerForm), Times.Never);
    }

    [Fact]
    public async Task LoginUser_ShouldReturnOk()
    {
        //Arrange
        var loginForm = new LoginForm() { Email = "test@example.com", Password = "password" };
        var expectedResult = new AuthResponse( true, "", "", "User logged in");
        _authenticateServiceMock.Setup(x => x.LoginUser(loginForm)).ReturnsAsync(expectedResult); 
        
        //Act 
        var result = await _authController.LoginUser(loginForm);
        var okResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.LoginUser(loginForm), Times.Once);
    }

    [Fact]
    public async Task LoginUser_ShouldReturnFalse()
    {
        //Arrange
        var loginForm = new LoginForm() { Email = "test@example.com", Password = "" };
        var expectedResult = new AuthResponse( false, "", "", "Login failed");
        _authenticateServiceMock.Setup(x => x.LoginUser(loginForm)).ReturnsAsync(expectedResult); 
        
        //Act 
        var result = await _authController.LoginUser(loginForm);
        var badResult = result as BadRequestObjectResult;

        //Assert
        Assert.NotNull(badResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.LoginUser(loginForm), Times.Once);
    }

    [Fact]
    public void ValidateToken_ShouldReturnOk()
    {
        //Arrange
        var token = "TestToken";
        var expectedResult = new ClaimsPrincipalResponse(true, new ClaimsPrincipal());
        _tokenServiceMock.Setup(x => x.ValidateToken(token)).Returns(expectedResult);
        
        //Act
        var result = _authController.ValidateToken(token);
        var okResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        _tokenServiceMock.Verify(x => x.ValidateToken(token), Times.Once);
    }

    [Fact]
    public void ValidateToken_ShouldReturnUnauthorised()
    {
        //Arrange
        var token = "TestToken";
        var expectedResult = new ClaimsPrincipalResponse(false, new ClaimsPrincipal());
        _tokenServiceMock.Setup(x => x.ValidateToken(token)).Returns(expectedResult);
        
        //Act
        var result = _authController.ValidateToken(token);
        var unauthorizedResult = result as UnauthorizedObjectResult;
        
        //Assert
        Assert.NotNull(unauthorizedResult);
        Assert.Equal((int)HttpStatusCode.Unauthorized, unauthorizedResult.StatusCode);
        _tokenServiceMock.Verify(x => x.ValidateToken(token), Times.Once);
    }
}

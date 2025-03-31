using System.ClientModel.Primitives;
using System.Net;
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
        var registerForm = new RegisterForm() { UserName = "testuser", Email = "test@example.com", Password = "password", Dob = DateOnly.FromDateTime(DateTime.Now) };
        var expectedResult = new AuthResponse( true, "", "", "User registered successfully");
        _authenticateServiceMock.Setup(x => x.CreateUser(registerForm)).ReturnsAsync(expectedResult);

        // Act
        var result = await _authController.RegisterUser(registerForm);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
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
    }
}
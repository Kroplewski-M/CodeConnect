using System.ClientModel.Primitives;
using System.Net;
using System.Security.Claims;
using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.AuthenticationEndpoint;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticateService> _authenticateServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthenticationController _authController;
    private readonly Mock<IHttpContextAccessor> _httpContextMock; 

    public AuthControllerTests()
    {
        _authenticateServiceMock = new Mock<IAuthenticateService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _httpContextMock = new Mock<IHttpContextAccessor>();
        
        var context = new DefaultHttpContext();
        _httpContextMock.Setup(x => x.HttpContext).Returns(context);
        _authController = new AuthenticationController(_authenticateServiceMock.Object, _tokenServiceMock.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = context.HttpContext }
        };
    }

    [Fact]
    public async Task RegisterUser_ShouldReturnOk()
    {
        // Arrange
        var registerForm = new RegisterForm() { FirstName = "Test",LastName = "Test",UserName = "testuser", Email = "test@example.com", Password = "password", Dob = DateOnly.FromDateTime(DateTime.UtcNow) };
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
        var registerForm = new RegisterForm() { UserName = "", Email = "", Password = "", Dob = DateOnly.FromDateTime(DateTime.UtcNow) };
        var expectedResult = new AuthResponse( false, "", "", "Invalid Register Form");
        _authenticateServiceMock.Setup(x => x.CreateUser(registerForm)).ReturnsAsync(expectedResult);

        // Act
        var result = await _authController.RegisterUser(registerForm);
        var badResult = result as BadRequestObjectResult;

        // Assert
        Assert.NotNull(badResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.CreateUser(registerForm), Times.Once);
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

    [Fact]
    public async Task RefreshToken_NoAuthorizedHeader_ShouldReturnUnauthorised()
    {
        //Act
        //Do nothing
        
        //Arrange
        var result = await _authController.RefreshToken();
        
        //Assert
        var unauthedResult = result as UnauthorizedObjectResult;
        Assert.NotNull(unauthedResult);
        Assert.Equal((int)HttpStatusCode.Unauthorized, unauthedResult.StatusCode);
        _tokenServiceMock.Verify(x => x.RefreshUserTokens(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_ValidRequest_noUser_ShouldReturnUnauthorised()
    {
        //Arrange 
        const string token = "refreshToken";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {token}";
        _httpContextMock.Setup(x => x.HttpContext).Returns(httpContext);
        var authControllerWithContext = new AuthenticationController(_authenticateServiceMock.Object, _tokenServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        //Act
        var result = await _authController.RefreshToken();
        
        //Assert
        var unauthedResult = result as UnauthorizedObjectResult;
        Assert.NotNull(unauthedResult);
        Assert.Equal((int)HttpStatusCode.Unauthorized, unauthedResult.StatusCode);
        _tokenServiceMock.Verify(x => x.RefreshUserTokens(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task RefreshToken_ValidRequest_TokenServiceSuccess_ShouldReturnOk()
    {
        // Arrange
        const string token = "refreshToken";
        const string username = "testuser";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {token}";
        var claims = new Claim[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        httpContext.User = claimsPrincipal;
        _httpContextMock.Setup(x => x.HttpContext).Returns(httpContext);
        var authControllerWithContext = new AuthenticationController(_authenticateServiceMock.Object, _tokenServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var expectedResponse = new AuthResponse(true, "","","");
        _tokenServiceMock.Setup(x => x.RefreshUserTokens(username, token)).ReturnsAsync(expectedResponse);

        // Act
        var result = await authControllerWithContext.RefreshToken();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        Assert.Equal(expectedResponse, okResult.Value);
        _tokenServiceMock.Verify(x => x.RefreshUserTokens(username, token), Times.Once);
    }

}

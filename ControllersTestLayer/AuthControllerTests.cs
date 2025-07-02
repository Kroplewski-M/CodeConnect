using System.ClientModel.Primitives;
using System.Net;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.AuthenticationEndpoint;
using DomainLayer.Constants;
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
        var deviceId = Guid.NewGuid().ToString();
        _authenticateServiceMock.Setup(x => x.CreateUser(registerForm,deviceId)).ReturnsAsync(expectedResult);

        // Act
        var result = await _authController.RegisterUser(registerForm,deviceId);
        var okResult = result as OkObjectResult;

        // Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.CreateUser(registerForm,deviceId), Times.Once);
    }
    [Fact]
    public async Task RegisterUser_ShouldReturnFalse()
    {
        // Arrange
        var registerForm = new RegisterForm() { UserName = "", Email = "", Password = "", Dob = DateOnly.FromDateTime(DateTime.UtcNow) };
        var expectedResult = new AuthResponse( false, "", "", "Invalid Register Form");
        var deviceId = Guid.NewGuid().ToString();
        _authenticateServiceMock.Setup(x => x.CreateUser(registerForm,deviceId)).ReturnsAsync(expectedResult);

        // Act
        var result = await _authController.RegisterUser(registerForm,deviceId);
        var badResult = result as BadRequestObjectResult;

        // Assert
        Assert.NotNull(badResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.CreateUser(registerForm,deviceId), Times.Once);
    }

    [Fact]
    public async Task LoginUser_ShouldReturnOk()
    {
        //Arrange
        var loginForm = new LoginForm() { Email = "test@example.com", Password = "password" };
        var expectedResult = new AuthResponse( true, "", "", "User logged in");
        var deviceId = Guid.NewGuid().ToString();
        _authenticateServiceMock.Setup(x => x.LoginUser(loginForm,deviceId)).ReturnsAsync(expectedResult); 
        
        //Act 
        var result = await _authController.LoginUser(loginForm,deviceId);
        var okResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.LoginUser(loginForm,deviceId), Times.Once);
    }

    [Fact]
    public async Task LoginUser_ShouldReturnFalse()
    {
        //Arrange
        var loginForm = new LoginForm() { Email = "test@example.com", Password = "" };
        var expectedResult = new AuthResponse( false, "", "", "Login failed");
        var deviceId = Guid.NewGuid().ToString();
        _authenticateServiceMock.Setup(x => x.LoginUser(loginForm,deviceId)).ReturnsAsync(expectedResult); 
        
        //Act 
        var result = await _authController.LoginUser(loginForm,deviceId);
        var badResult = result as BadRequestObjectResult;

        //Assert
        Assert.NotNull(badResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badResult.StatusCode);
        _authenticateServiceMock.Verify(x => x.LoginUser(loginForm, deviceId), Times.Once);
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
        var result = await _authController.RefreshToken(Guid.NewGuid().ToString());
        
        //Assert
        var unauthedResult = result as UnauthorizedObjectResult;
        Assert.NotNull(unauthedResult);
        Assert.Equal((int)HttpStatusCode.Unauthorized, unauthedResult.StatusCode);
        _tokenServiceMock.Verify(x => x.RefreshUserTokens( It.IsAny<string>(),It.IsAny<string>()), Times.Never);
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
        var result = await _authController.RefreshToken(Guid.NewGuid().ToString());
        
        //Assert
        var unauthedResult = result as UnauthorizedObjectResult;
        Assert.NotNull(unauthedResult);
        Assert.Equal((int)HttpStatusCode.Unauthorized, unauthedResult.StatusCode);
        _tokenServiceMock.Verify(x => x.RefreshUserTokens( It.IsAny<string>(),It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task RefreshToken_ValidRequest_TokenServiceSuccess_ShouldReturnOk()
    {
        // Arrange
        const string token = "refreshToken";
        const string username = "testuser";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {token}";
        var claims = new Claim[] { new Claim(Consts.ClaimTypes.UserName, username) };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        httpContext.User = claimsPrincipal;
        _httpContextMock.Setup(x => x.HttpContext).Returns(httpContext);
        var authControllerWithContext = new AuthenticationController(_authenticateServiceMock.Object, _tokenServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var expectedResponse = new AuthResponse(true, "","","");
        var deviceId = Guid.NewGuid().ToString();
        _tokenServiceMock.Setup(x => x.RefreshUserTokens(token,deviceId)).ReturnsAsync(expectedResponse);
        _tokenServiceMock.Setup(x => x.ValidateToken(token)).Returns(new ClaimsPrincipalResponse(true, claimsPrincipal));
        // Act
        var result = await authControllerWithContext.RefreshToken(deviceId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        Assert.Equal(expectedResponse, okResult.Value);
        _tokenServiceMock.Verify(x => x.RefreshUserTokens(token,deviceId), Times.Once);
    }

}

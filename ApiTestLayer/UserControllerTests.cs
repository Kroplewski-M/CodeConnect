using System.Net;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.UserEndpoint;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IUserImageService> _userImageServiceMock;
    private readonly UserController _userController;
    public UserControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, 
            null!, null!, null!, null!, null!, null!, null!, null!
        );
        _userServiceMock = new Mock<IUserService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _userImageServiceMock = new Mock<IUserImageService>();
        _userController = new UserController(_userServiceMock.Object, _userManagerMock.Object, _tokenServiceMock.Object, _userImageServiceMock.Object);
        
        var currentUsername = "testUsername";
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, currentUsername) };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _userController.ControllerContext = new ControllerContext(){ HttpContext = httpContext };
    }

    [Fact]
    public async Task EditUserDetails_EmptyUsername_ReturnsBadRequest()
    {
        //Arrange
        EditProfileForm editForm = new EditProfileForm()
        {
            Username = "",
            Bio = "",
            DOB = DateOnly.FromDateTime(DateTime.Now),
            FirstName = "",
            LastName = "",
            GithubLink = "",
            WebsiteLink = "",
        };
        //Act
        var result = await _userController.EditUserDetails(editForm);
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);  
        Assert.Equal(badRequestResult.StatusCode, (int)HttpStatusCode.BadRequest);
        _userServiceMock.Verify(x=> x.UpdateUserDetails(It.IsAny<EditProfileForm>()), Times.Never);
    }
    [Fact]
    public async Task EditUserDetails_WrongUsername_ReturnsBadRequest()
    {
        //Arrange
        EditProfileForm editForm = new EditProfileForm()
        {
            Username = "testUsername",
            Bio = "",
            DOB = DateOnly.FromDateTime(DateTime.Now),
            FirstName = "",
            LastName = "",
            GithubLink = "",
            WebsiteLink = "",
        };
        _userManagerMock.Setup(x=> x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "WrongUsername"});
        //Act
        var result = await _userController.EditUserDetails(editForm);
        var unAuthorizedRequestResult = result as UnauthorizedObjectResult;
        
        //Assert
        Assert.NotNull(unAuthorizedRequestResult);  
        Assert.Equal(unAuthorizedRequestResult.StatusCode, (int)HttpStatusCode.Unauthorized);
        _userServiceMock.Verify(x=> x.UpdateUserDetails(It.IsAny<EditProfileForm>()), Times.Never);
    }

    [Fact]
    public async Task EditUserDetails_GoodForm_CorrectUser_ShouldReturnOk()
    {
        //Arrange
        EditProfileForm editForm = new EditProfileForm()
        {
            Username = "testUsername",
            Bio = "",
            DOB = DateOnly.FromDateTime(DateTime.Now),
            FirstName = "",
            LastName = "",
            GithubLink = "",
            WebsiteLink = "",
        };
        var expectedResult = new ServiceResponse(true,"");
        _userManagerMock.Setup(x=> x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "testUsername"});
        _userServiceMock.Setup(x=> x.UpdateUserDetails(editForm)).ReturnsAsync(expectedResult);
        //Act
        var result = await _userController.EditUserDetails(editForm);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userServiceMock.Verify(x=> x.UpdateUserDetails(It.IsAny<EditProfileForm>()), Times.Once);
    }

    [Fact]
    public async Task GetUserDetails_NoUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var username = "";
        
        //Act
        var result = await _userController.GetUserDetails(username);
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest,badRequestResult.StatusCode);
        _userServiceMock.Verify(x=> x.GetUserDetails(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserDetails_CorrectUsername_ShouldReturnOk()
    {
        //Arrange
        var username = "testUsername";
        var expectedResult = new UserDetails("","","testUsername","","","","","",null,null,"");
        _userServiceMock.Setup(x => x.GetUserDetails(username)).ReturnsAsync(expectedResult);
        
        //Act
        var result = await _userController.GetUserDetails(username);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userServiceMock.Verify(x=> x.GetUserDetails(It.IsAny<string>()), Times.Once);
    }
}
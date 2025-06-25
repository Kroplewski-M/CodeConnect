using System.Net;
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.UserEndpoint;
using DomainLayer.Constants;
using DomainLayer.DbEnts;
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
    private readonly string _testUserId = "user123";
    private readonly string _testUsername = "testUsername";
    
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
        
        // Setup claims for authenticated user
        var claims = new[]
        {
            new Claim(Consts.ClaimTypes.UserName, _testUsername),
            new Claim(Consts.ClaimTypes.Id, _testUserId)
        };
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
            Dob = DateOnly.FromDateTime(DateTime.UtcNow),
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
            Dob = DateOnly.FromDateTime(DateTime.UtcNow),
            FirstName = "",
            LastName = "",
            GithubLink = "",
            WebsiteLink = "",
        };
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "WrongUsername"});
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
            Dob = DateOnly.FromDateTime(DateTime.UtcNow),
            FirstName = "",
            LastName = "",
            GithubLink = "",
            WebsiteLink = "",
        };
        var expectedResult = new ServiceResponse(true,"");
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "testUsername"});
        _userServiceMock.Setup(x => x.UpdateUserDetails(editForm)).ReturnsAsync(expectedResult);
        
        //Act
        var result = await _userController.EditUserDetails(editForm);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userServiceMock.Verify(x => x.UpdateUserDetails(It.IsAny<EditProfileForm>()), Times.Once);
    }

    [Fact]
    public async Task GetUserDetails_NoUserIdInClaims_ShouldReturnBadRequest()
    {
        //Arrange
        // Create new controller with no ID claim
        var controller = new UserController(_userServiceMock.Object, _userManagerMock.Object, _tokenServiceMock.Object, _userImageServiceMock.Object);
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, _testUsername) }; // Only username, no ID
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        controller.ControllerContext = new ControllerContext(){ HttpContext = httpContext };
        
        //Act
        var result = await controller.GetUserDetails();
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _userServiceMock.Verify(x => x.GetUserDetails(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserDetails_WithUserId_ShouldReturnOk()
    {
        //Arrange
        var expectedResult = new UserDetails("","", _testUsername,"","","","","",null,null,"");
        _userServiceMock.Setup(x => x.GetUserDetails(_testUserId)).ReturnsAsync(expectedResult);
        
        //Act
        var result = await _userController.GetUserDetails();
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userServiceMock.Verify(x => x.GetUserDetails(_testUserId), Times.Once);
    }

    [Fact]
    public async Task UpdateUserImage_NoUserIdInClaims_ShouldReturnUnauthorized()
    {
        //Arrange
        // Create new controller with no ID claim
        var controller = new UserController(_userServiceMock.Object, _userManagerMock.Object, _tokenServiceMock.Object, _userImageServiceMock.Object);
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, _testUsername) }; // Only username, no ID
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        controller.ControllerContext = new ControllerContext(){ HttpContext = httpContext };
        
        var editUserImg = new UpdateUserImageRequest()
        {
            FileName = "randomFileName",
            ImgBase64 = "randomImage",
            TypeOfImage = Consts.ImageType.ProfileImages
        };
        
        //Act
        var result = await controller.UpdateUserImage(editUserImg);
        var unAuthorizedRequestResult = result as UnauthorizedObjectResult;
        
        //Assert
        Assert.NotNull(unAuthorizedRequestResult);
        Assert.Equal((int)HttpStatusCode.Unauthorized, unAuthorizedRequestResult.StatusCode);
        _userImageServiceMock.Verify(x => x.UpdateUserImage(It.IsAny<UpdateUserImageRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserImage_NoImage_ShouldReturnBadRequest()
    {
        //Arrange
        var editUserImg = new UpdateUserImageRequest()
        {
            FileName = "",
            ImgBase64 = "",
            TypeOfImage = Consts.ImageType.ProfileImages
        };
        
        //Act
        var result = await _userController.UpdateUserImage(editUserImg);
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _userImageServiceMock.Verify(x => x.UpdateUserImage(It.IsAny<UpdateUserImageRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserImage_ImagePopulated_NoFileName_ShouldReturnBadRequest()
    {
        //Arrange
        var editUserImg = new UpdateUserImageRequest()
        {
            FileName = "",
            ImgBase64 = "randomImage",
            TypeOfImage = Consts.ImageType.ProfileImages
        };
        
        //Act
        var result = await _userController.UpdateUserImage(editUserImg);
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _userImageServiceMock.Verify(x => x.UpdateUserImage(It.IsAny<UpdateUserImageRequest>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateUserImage_AllDataPopulated_ShouldReturnOk()
    {
        //Arrange
        var editUserImg = new UpdateUserImageRequest()
        {
            FileName = "randomFileName",
            ImgBase64 = "randomImage",
            TypeOfImage = Consts.ImageType.ProfileImages
        };
        var expectedResponse = new ServiceResponse(true,"");
        _userImageServiceMock.Setup(x => x.UpdateUserImage(It.IsAny<UpdateUserImageRequest>()))
            .ReturnsAsync(expectedResponse);
        
        //Act
        var result = await _userController.UpdateUserImage(editUserImg);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userImageServiceMock.Verify(x => x.UpdateUserImage(It.Is<UpdateUserImageRequest>(
            req => req.UserId == _testUserId)), Times.Once);
    }

    [Fact]
    public async Task GetUserInterests_NoUserIdInClaims_ShouldReturnBadRequest()
    {
        //Arrange
        // Create new controller with no ID claim
        var controller = new UserController(_userServiceMock.Object, _userManagerMock.Object, _tokenServiceMock.Object, _userImageServiceMock.Object);
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, _testUsername) }; // Only username, no ID
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        controller.ControllerContext = new ControllerContext(){ HttpContext = httpContext };
        
        //Act
        var result = await controller.GetUserInterests();
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        _userServiceMock.Verify(x => x.GetUserInterests(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserInterests_WithUserId_ShouldReturnOk()
    {
        //Arrange
        _userServiceMock.Setup(x => x.GetUserInterests(_testUserId))
            .ReturnsAsync(new UserInterestsDto(true, "", new List<TechInterestsDto>()));
        
        //Act
        var result = await _userController.GetUserInterests();
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userServiceMock.Verify(x => x.GetUserInterests(_testUserId), Times.Once);
    }
    [Fact]
    public async Task GetAllInterests_ShouldReturnOk()
    {
        //Arrange
        var interest = new TechInterestsDto(1, 1, "type", "name");
        _userServiceMock.Setup(x => x.GetAllInterests()).ReturnsAsync(new List<TechInterestsDto>() { interest });
        
        //Act
        var result = await _userController.GetAllInterests();
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal((int)HttpStatusCode.OK, okRequestResult.StatusCode);
        _userServiceMock.Verify(x => x.GetAllInterests(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllInterests_ReturnsEmpty_ShouldReturnServiceUnavailable()
    {
        //Arrange
        _userServiceMock.Setup(x => x.GetAllInterests()).ReturnsAsync(new List<TechInterestsDto>());
        
        //Act
        var result = await _userController.GetAllInterests();

        //Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, objectResult.StatusCode);
        _userServiceMock.Verify(x => x.GetAllInterests(), Times.Once);
    }
}
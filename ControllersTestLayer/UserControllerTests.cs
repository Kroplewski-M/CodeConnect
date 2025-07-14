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
        _userController = new UserController(_userServiceMock.Object, _userImageServiceMock.Object);
        
        var currentUsername = "testUsername";
        var currentId = Guid.NewGuid().ToString();
        SetMockUserInContext(currentUsername, currentId);
    }
    private void SetMockUserInContext(string username, string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(Consts.ClaimTypes.UserName, username),
            new Claim(Consts.ClaimTypes.Id, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task EditUserDetails_GoodForm_CorrectUser_ShouldReturnOk()
    {
        //Arrange
        EditProfileForm editForm = new EditProfileForm()
        {
            Bio = "",
            Dob = DateOnly.FromDateTime(DateTime.UtcNow),
            FirstName = "",
            LastName = "",
            GithubLink = "",
            WebsiteLink = "",
        };
        var expectedResult = new ServiceResponse(true,"");
        _userManagerMock.Setup(x=> x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "testUsername"});
        _userServiceMock.Setup(x=> x.UpdateUserDetails(editForm,It.IsAny<string>())).ReturnsAsync(expectedResult);
        //Act
        var result = await _userController.EditUserDetails(editForm);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userServiceMock.Verify(x=> x.UpdateUserDetails(It.IsAny<EditProfileForm>(),It.IsAny<string>()), Times.Once);
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
        var expectedResult = new UserDetails("","","","testUsername","","","","","",null,null,"");
        _userServiceMock.Setup(x => x.GetUserDetails(username)).ReturnsAsync(expectedResult);
        
        //Act
        var result = await _userController.GetUserDetails(username);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal(okRequestResult.StatusCode, (int)HttpStatusCode.OK);
        _userServiceMock.Verify(x=> x.GetUserDetails(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task UpdateUserImage_CorrectUsername_NoImage_ShouldReturnBadRequest()
    {
        //Arrange
        var editUserImg = new UpdateUserImageRequest()
        {
            FileName = "",
            ImgBase64 = "",
            TypeOfImage = Consts.ImageType.ProfileImages,
        };
        //Act
        var result = await _userController.UpdateUserImage(editUserImg);
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest,badRequestResult.StatusCode);
        _userImageServiceMock.Verify(x=> x.UpdateUserImage(It.IsAny<UpdateUserImageRequest>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserImage_CorrectUsername_ImagePopulated_NoFileName_ShouldReturnBadRequest()
    {
        //Arrange
        var editUserImg = new UpdateUserImageRequest()
        {
            FileName = "",
            ImgBase64 = "randomImage",
            TypeOfImage = Consts.ImageType.ProfileImages,
        };
        //Act
        var result = await _userController.UpdateUserImage(editUserImg);
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest,badRequestResult.StatusCode);
        _userImageServiceMock.Verify(x=> x.UpdateUserImage(It.IsAny<UpdateUserImageRequest>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserImage_AllDataPopulated_ShouldReturnOk()
    {
        //Arrange
        var editUserImg = new UpdateUserImageRequest()
        {
            FileName = "randomFileName",
            ImgBase64 = "randomImage",
            TypeOfImage = Consts.ImageType.ProfileImages,
        };
        var expectedResponse = new ServiceResponse(true,"");
        _userImageServiceMock.Setup(x=> x.UpdateUserImage(editUserImg, It.IsAny<string>())).ReturnsAsync(expectedResponse);
        _userManagerMock.Setup(x=> x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser(){UserName = "testUsername"});
        //Act
        var result = await _userController.UpdateUserImage(editUserImg);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal((int)HttpStatusCode.OK,okRequestResult.StatusCode);
        _userImageServiceMock.Verify(x=> x.UpdateUserImage(It.IsAny<UpdateUserImageRequest>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetUserInterests_NoUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var username = "";
        
        //Act
        var result = await _userController.GetUserInterests(username);
        var badRequestResult = result as BadRequestObjectResult;
        
        //Assert
        Assert.NotNull(badRequestResult);
        Assert.Equal((int)HttpStatusCode.BadRequest,badRequestResult.StatusCode);
        _userServiceMock.Verify(x=> x.GetUserInterests(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserInterests_UsernameProvided_ShouldReturnOk()
    {
        //Arrange
        var username = "someUsername";
        _userServiceMock.Setup(x=> x.GetUserInterests(username)).ReturnsAsync(new UserInterestsDto(true,"",new List<TechInterestsDto>()));
        
        //Act
        var result = await _userController.GetUserInterests(username);
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal((int)HttpStatusCode.OK,okRequestResult.StatusCode);
        _userServiceMock.Verify(x=> x.GetUserInterests(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAllInterests_ShouldReturnOk()
    {
        //Arrange
        var interest = new TechInterestsDto(1,1,"type","name");
        _userServiceMock.Setup(x=> x.GetAllInterests()).ReturnsAsync(new List<TechInterestsDto>() { interest });
        
        //Act
        var result = await _userController.GetAllInterests();
        var okRequestResult = result as OkObjectResult;
        
        //Assert
        Assert.NotNull(okRequestResult);
        Assert.Equal((int)HttpStatusCode.OK,okRequestResult.StatusCode);
        _userServiceMock.Verify(x=> x.GetAllInterests(), Times.Once);
    }
    [Fact]
    public async Task GetAllInterests_ReturnsEmpty_ShouldReturnServiceUnavailable()
    {
        //Arrange
        _userServiceMock.Setup(x=> x.GetAllInterests()).ReturnsAsync(new List<TechInterestsDto>());
        
        //Act
        var result = await _userController.GetAllInterests();

        //Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, objectResult.StatusCode);
        _userServiceMock.Verify(x=> x.GetAllInterests(), Times.Once);
    }
}
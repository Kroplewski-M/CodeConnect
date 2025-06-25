using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ApiServiceTests;

public class UserImageServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IAzureService> _azureServiceMock;
    private readonly IUserImageService _userImageService;

    public UserImageServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _azureServiceMock = new Mock<IAzureService>();
        _userImageService = new UserImageService(_userManagerMock.Object, _azureServiceMock.Object);
    }
    [Fact]
    public async Task UpdateUserImage_UserNotFound_ReturnsErrorResponse()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        var request = new UpdateUserImageRequest { UserId = Guid.NewGuid().ToString(), TypeOfImage = Consts.ImageType.PostImages, FileName = "something", ImgBase64 = "someBase"};

        // Act
        var result = await _userImageService.UpdateUserImage(request);

        // Assert
        Assert.False(result.Flag);
        _azureServiceMock.Verify(x=> x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserImage_NoUsername_ReturnsErrorResponse()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        var request = new UpdateUserImageRequest { UserId = Guid.NewGuid().ToString(), TypeOfImage = Consts.ImageType.PostImages, FileName = "something", ImgBase64 = "someBase"};

        // Act
        var result = await _userImageService.UpdateUserImage(request);

        // Assert
        Assert.False(result.Flag);
        _azureServiceMock.Verify(x=> x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserImage_NoFileName_ReturnsErrorResponse()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        var request = new UpdateUserImageRequest {UserId = Guid.NewGuid().ToString(), TypeOfImage = Consts.ImageType.PostImages, FileName = "", ImgBase64 = "someBase"};

        // Act
        var result = await _userImageService.UpdateUserImage(request);

        // Assert
        Assert.False(result.Flag);
        _azureServiceMock.Verify(x=> x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserImage_NoBase64Image_ReturnsErrorResponse()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        var request = new UpdateUserImageRequest { UserId = Guid.NewGuid().ToString(), TypeOfImage = Consts.ImageType.PostImages, FileName = "fileName", ImgBase64 = ""};

        // Act
        var result = await _userImageService.UpdateUserImage(request);

        // Assert
        Assert.False(result.Flag);
        _azureServiceMock.Verify(x=> x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task UpdateUserImage_UploadFails_ReturnsFailure()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(),UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

        _azureServiceMock.Setup(x => x.RemoveImage(It.IsAny<string>(), It.IsAny<Consts.ImageType>()))
            .ReturnsAsync(new ServiceResponse(true, ""));

        _azureServiceMock.Setup(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AzureImageDto(false, "", "failed"));

        var request = new UpdateUserImageRequest
        {
            UserId = Guid.NewGuid().ToString(),
            FileName = "test.png",
            ImgBase64 = "base64string",
            TypeOfImage = Consts.ImageType.ProfileImages
        };

        var result = await _userImageService.UpdateUserImage(request);

        Assert.False(result.Flag);
    }
    [Fact]
    public async Task UpdateUserImage_Success_UpdatesUserAndReturnsSuccess()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(),UserName = "testuser" };
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

        _azureServiceMock.Setup(x => x.RemoveImage(It.IsAny<string>(), It.IsAny<Consts.ImageType>()))
            .ReturnsAsync(new ServiceResponse(true, "No image is needing deletion"));

        _azureServiceMock.Setup(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AzureImageDto(true, "NewName", "success"));

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        var request = new UpdateUserImageRequest
        {
            UserId = Guid.NewGuid().ToString(),
            FileName = "test.png",
            ImgBase64 = "base64string",
            TypeOfImage = Consts.ImageType.ProfileImages
        };

        var result = await _userImageService.UpdateUserImage(request);

        Assert.True(result.Flag);
        _azureServiceMock.Verify(x=> x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public async Task UpdateUserImage_UserHasOldProfileImage_RemovesOldImage()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            ProfileImage = "old-image.png"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

        // Assert RemoveImage is called
        _azureServiceMock.Setup(x => x.RemoveImage("old-image.png", Consts.ImageType.ProfileImages))
            .ReturnsAsync(new ServiceResponse(true, "Image removed"))
            .Verifiable();

        _azureServiceMock.Setup(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AzureImageDto(true, "name", "success"));

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        var request = new UpdateUserImageRequest
        {
            UserId = Guid.NewGuid().ToString(),
            FileName = "file.jpg",
            ImgBase64 = "base64string",
            TypeOfImage = Consts.ImageType.ProfileImages
        };

        // Act
        var result = await _userImageService.UpdateUserImage(request);

        // Assert
        Assert.True(result.Flag);
        _azureServiceMock.Verify(); 
    }
}

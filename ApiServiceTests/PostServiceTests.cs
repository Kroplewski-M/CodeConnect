using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ApiServiceTests;
[Collection("DatabaseCollection")]
public class PostServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAzureService> _azureService;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly PostService _postService;
    public PostServiceTests(DatabaseFixture databaseFixture)
    {
        _context = databaseFixture.Context;
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!,
            null!, null!, null!);
        _azureService = new Mock<IAzureService>();
        _postService = new PostService(_context, _azureService.Object, _userManager.Object);
    }

    [Fact]
    public async Task CreatePost_NoContent_ShouldReturn_BadServiceResponse()
    {
        //Arrange
        var request = new CreatePostDto("", [], "username");
        //Act
        var response = await _postService.CreatePost(request);
        
        //Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.NotEmpty(response.Message);
        _azureService.Verify(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task CreatePost_NoUsername_ShouldReturn_BadServiceResponse()
    {
        //Arrange
        var request = new CreatePostDto("some content", [], "");
        //Act
        var response = await _postService.CreatePost(request);
        
        //Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.NotEmpty(response.Message);
        _azureService.Verify(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task CreatePost_CannotFindUser_ShouldReturn_BadServiceResponse()
    {
        //Arrange
        var request = new CreatePostDto("some content", [], "SomeUsername");
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as ApplicationUser);
        //Act
        var response = await _postService.CreatePost(request);
        
        //Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.NotEmpty(response.Message);
        _azureService.Verify(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreatePost_NoImages_ShouldOnlyCreatePost()
    {
        //Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser() {Id = userId, UserName = "user"};
        var request = new CreatePostDto("some content", null, user.UserName);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
        //Act
        var response = await _postService.CreatePost(request);
        //Assert
        Assert.NotNull(response);
        Assert.True(response.Flag);
        Assert.NotEmpty(response.Message);
        Assert.NotNull(_context.Posts.FirstOrDefault(x=> x.CreatedByUserId == userId));
        _azureService.Verify(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task CreatePost_WithImages_ShouldOnlyCreatePost()
    {
        //Arrange
        var base64Images = new List<Base64Dto>()
        {
            new Base64Dto("SGVsbG8gd29ybGQh", ".png"),
            new Base64Dto("SGVsbG8gd29ybGQh", ".png"),
            new Base64Dto("SGVsbG8gd29ybGQh", ".png"),
        };
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser() {Id = userId, UserName = "user"};
        var request = new CreatePostDto("some content", base64Images, user.UserName);
        _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
        _azureService.Setup(x =>
                x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AzureImageDto(true, "name", ""));
        //Act
        var response = await _postService.CreatePost(request);
        //Assert
        Assert.NotNull(response);
        Assert.True(response.Flag);
        Assert.NotEmpty(response.Message);
        var post = _context.Posts.Include(post => post.Files).FirstOrDefault(x => x.CreatedByUserId == userId);
        Assert.NotNull(post);
        Assert.Equal(base64Images.Count, post.Files?.Count);
        _azureService.Verify(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
    }
}
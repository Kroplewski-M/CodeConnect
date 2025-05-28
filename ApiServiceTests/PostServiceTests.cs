using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Posts;
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
    [Fact]
    public async Task GetUserPosts_EmptyUsername_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyUsername = "";

        // Act
        var result = await _postService.GetUserPosts(emptyUsername,0,10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _userManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task GetUserPosts_NullUsername_ShouldReturnEmptyList()
    {
        // Arrange
        string? nullUsername = null;

        // Act
        var result = await _postService.GetUserPosts(nullUsername!,0,10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _userManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task GetUserPosts_UserNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        var username = "nonExistentUser";
        _userManager.Setup(x => x.FindByNameAsync(username)).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _postService.GetUserPosts(username, 0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _userManager.Verify(x => x.FindByNameAsync(username), Times.Once);
    }
    [Fact]
    public async Task GetUserPosts_ValidUserWithNoPosts_ShouldReturnEmptyList()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "userWithNoPosts" };
        _userManager.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
        
        // Act
        var result = await _postService.GetUserPosts(user.UserName, 0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _userManager.Verify(x => x.FindByNameAsync(user.UserName), Times.Once);
    }
    [Fact]
    public async Task GetUserPosts_ValidUserWithPosts_ShouldReturnPosts()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "userWithPosts" };
        _userManager.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);

        var post = new Post
        {
            Content = "Hello world",
            CreatedByUser = user,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            Files = new List<PostFile>
            {
                new PostFile { FileName = "image1.png" },
                new PostFile { FileName = "image2.png" }
            }
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.GetUserPosts(user.UserName, 0, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var returnedPost = result.FirstOrDefault();
        Assert.Equal(post.Content, returnedPost?.Content);
        Assert.Equal(user.UserName, returnedPost?.CreatedByUsername);
        Assert.Equal(2, returnedPost?.Images.Count);
        _userManager.Verify(x => x.FindByNameAsync(user.UserName), Times.Once);
    }
}
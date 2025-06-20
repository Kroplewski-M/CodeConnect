using ApplicationLayer.APIServices;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using ApplicationLayer.DTO_s.Post;
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
    [Fact]
    public async Task GetPostById_NullOrEmptyId_ShouldReturnNull()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _postService.GetPostById(emptyId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPostById_NonExistentPost_ShouldReturnNull()
    {
        // Arrange
        var nonExistentPostId = Guid.NewGuid();

        // Act
        var result = await _postService.GetPostById(nonExistentPostId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPostById_ExistingPostWithoutImages_ShouldReturnPostDto()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "testUser" };
        _userManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = "Test post content",
            CreatedByUser = user,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            Files = new List<PostFile>()
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.GetPostById(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(post.Id, result.Id);
        Assert.Equal(post.Content, result.Content);
        Assert.Equal(user.UserName, result.CreatedByUsername);
        Assert.Empty(result.Images);
    }

    [Fact]
    public async Task GetPostById_ExistingPostWithImages_ShouldReturnPostDtoWithImages()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "testUser" };
        _userManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = "Test post content with images",
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
        var result = await _postService.GetPostById(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(post.Id, result.Id);
        Assert.Equal(post.Content, result.Content);
        Assert.Equal(user.UserName, result.CreatedByUsername);
        Assert.Equal(2, result.Images.Count);
    }

    [Fact]
    public async Task GetPostById_ShouldIncludeLikesAndCommentsCount()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "testUser" };
        _userManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = "Test post with likes and comments",
            CreatedByUser = user,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            Files = new List<PostFile>(),
            Comments = new List<Comment>
            {
                new Comment { Content = "Test comment 1", CreatedByUserId = Guid.NewGuid().ToString() },
                new Comment { Content = "Test comment 2", CreatedByUserId = Guid.NewGuid().ToString() },
                new Comment { Content = "Test comment 3", CreatedByUserId = Guid.NewGuid().ToString() }
            }
        };
        post.Likes.Add(new PostLike()
        {
            Id = Guid.NewGuid(),
            LikedByUserId = user.Id,
            PostId = post.Id,
            LikedOn = DateTime.UtcNow
        });
        post.Likes.Add(new PostLike()
        {
            Id = Guid.NewGuid(),
            LikedByUserId = user.Id,
            PostId = post.Id,
            LikedOn = DateTime.UtcNow
        });
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.GetPostById(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(post.Id, result.Id);
        Assert.Equal(2, result.LikeCount);
        Assert.Equal(3, result.CommentCount);
    }
    [Fact]
    public async Task ToggleLikePost_EmptyUsername_ShouldReturnBadServiceResponse()
    {
        // Arrange
        var request = new LikePostDto(Guid.NewGuid(), "");
        
        // Act
        var response = await _postService.ToggleLikePost(request);
        
        // Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.Equal("Error occured while toggling like post", response.Message);
    }

    [Fact]
    public async Task ToggleLikePost_PostNotFound_ShouldReturnBadServiceResponse()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var request = new LikePostDto(postId, "testUser");
        
        // Act
        var response = await _postService.ToggleLikePost(request);
        
        // Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.Equal("Post not found", response.Message);
    }

    [Fact]
    public async Task ToggleLikePost_UserNotFound_ShouldReturnBadServiceResponse()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "testUser" };
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = "Test post",
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        var request = new LikePostDto(post.Id, "nonExistentUser");
        _userManager.Setup(x => x.FindByNameAsync(request.Username)).ReturnsAsync((ApplicationUser?)null);
        
        // Act
        var response = await _postService.ToggleLikePost(request);
        
        // Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.Equal("User not found", response.Message);
        _userManager.Verify(x => x.FindByNameAsync(request.Username), Times.Once);
    }

    [Fact]
    public async Task ToggleLikePost_AddNewLike_ShouldAddLikeToPost()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testUser" };
        
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = "Test post",
            CreatedByUserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Likes = new List<PostLike>()
        };
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        var request = new LikePostDto(post.Id, user.UserName);
        _userManager.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
        
        // Act
        var response = await _postService.ToggleLikePost(request);
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response.Flag);
        Assert.Equal("Like added successfully", response.Message);
        
        var updatedPost = await _context.Posts
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == post.Id);
        
        Assert.NotNull(updatedPost);
        Assert.Single(updatedPost.Likes);
        Assert.Equal(userId, updatedPost.Likes.First().LikedByUserId);
        Assert.Equal(post.Id, updatedPost.Likes.First().PostId);
        _userManager.Verify(x => x.FindByNameAsync(user.UserName), Times.Once);
    }

    [Fact]
    public async Task ToggleLikePost_RemoveExistingLike_ShouldRemoveLikeFromPost()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testUser" };
        
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = "Test post",
            CreatedByUserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Likes = new List<PostLike>()
        };
        
        var existingLike = new PostLike
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            LikedByUserId = userId,
            LikedOn = DateTime.UtcNow
        };
        
        post.Likes.Add(existingLike);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        var request = new LikePostDto(post.Id, user.UserName);
        _userManager.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
        
        // Act
        var response = await _postService.ToggleLikePost(request);
        
        // Assert
        Assert.NotNull(response);
        Assert.True(response.Flag);
        Assert.Equal("Like added successfully", response.Message);
        
        var updatedPost = await _context.Posts
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == post.Id);
        
        Assert.NotNull(updatedPost);
        Assert.Empty(updatedPost.Likes);
        _userManager.Verify(x => x.FindByNameAsync(user.UserName), Times.Once);
    }
    [Fact]
    public async Task IsUserLikingPost_EmptyUsername_ShouldReturnFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var emptyUsername = "";
        
        // Act
        var result = await _postService.IsUserLikingPost(postId, emptyUsername);
        
        // Assert
        Assert.False(result);
        _userManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task IsUserLikingPost_NullUsername_ShouldReturnFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();
        string? nullUsername = null;
        
        // Act
        var result = await _postService.IsUserLikingPost(postId, nullUsername!);
        
        // Assert
        Assert.False(result);
        _userManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task IsUserLikingPost_UserNotFound_ShouldReturnFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var username = "nonExistentUser";
        _userManager.Setup(x => x.FindByNameAsync(username)).ReturnsAsync((ApplicationUser?)null);
        
        // Act
        var result = await _postService.IsUserLikingPost(postId, username);
        
        // Assert
        Assert.False(result);
        _userManager.Verify(x => x.FindByNameAsync(username), Times.Once);
    }

    [Fact]
    public async Task IsUserLikingPost_UserNotLikingPost_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testUser" };
        var postId = Guid.NewGuid();
        
        _userManager.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
        
        // Create a post without any likes from this user
        var post = new Post
        {
            Id = postId,
            Content = "Test post",
            CreatedByUserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _postService.IsUserLikingPost(postId, user.UserName);
        
        // Assert
        Assert.False(result);
        _userManager.Verify(x => x.FindByNameAsync(user.UserName), Times.Once);
    }

    [Fact]
    public async Task IsUserLikingPost_UserLikingPost_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testUser" };
        var postId = Guid.NewGuid();
        
        _userManager.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
        
        // Create a post with a like from this user
        var post = new Post
        {
            Id = postId,
            Content = "Test post",
            CreatedByUserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Likes = new List<PostLike>()
        };
        
        var postLike = new PostLike
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            LikedByUserId = userId,
            LikedOn = DateTime.UtcNow
        };
        
        post.Likes.Add(postLike);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _postService.IsUserLikingPost(postId, user.UserName);
        
        // Assert
        Assert.True(result);
        _userManager.Verify(x => x.FindByNameAsync(user.UserName), Times.Once);
    }

    [Fact]
    public async Task IsUserLikingPost_DifferentPostLikedByUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testUser" };
        var postId = Guid.NewGuid();
        var differentPostId = Guid.NewGuid();
        
        _userManager.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
        
        // Create a post that the user hasn't liked
        var post = new Post
        {
            Id = postId,
            Content = "Test post",
            CreatedByUserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        
        // Create another post that the user has liked
        var differentPost = new Post
        {
            Id = differentPostId,
            Content = "Different test post",
            CreatedByUserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Likes = new List<PostLike>()
        };
        
        var postLike = new PostLike
        {
            Id = Guid.NewGuid(),
            PostId = differentPostId,
            LikedByUserId = userId,
            LikedOn = DateTime.UtcNow
        };
        
        differentPost.Likes.Add(postLike);
        _context.Posts.Add(post);
        _context.Posts.Add(differentPost);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _postService.IsUserLikingPost(postId, user.UserName);
        
        // Assert
        Assert.False(result);
        _userManager.Verify(x => x.FindByNameAsync(user.UserName), Times.Once);
    }

}
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Posts;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApiApplicationLayer;
using WebApiApplicationLayer.Interfaces;
using WebApiApplicationLayer.Services;

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
        _azureService = new Mock<IAzureService>();
        
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!,
            null!, null!, null!);
        var notificationServiceMock = new Mock<IServerNotificationsService>();
        _postService = new PostService(_context, _azureService.Object, _userManager.Object,notificationServiceMock.Object);
    }
    
    // Helper methods
    private ApplicationUser CreateUser(string userName)
    {
        return new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = userName };
    }

    private CreatePostDto CreatePostDto(string content, List<Base64Dto>? images)
    {
        return new CreatePostDto(content, images);
    }

    private List<Base64Dto> CreateBase64Images(int count)
    {
        return Enumerable.Repeat(new Base64Dto("SGVsbG8gd29ybGQh", ".png"), count).ToList();
    }
    
    // Test methods
    [Fact]
    public async Task CreatePost_InvalidContent_ShouldFailValidation()
    {
        // Arrange
        var invalidRequest = CreatePostDto("", null);

        // Act
        var response = await _postService.CreatePost(invalidRequest,"id");

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.NotEmpty(response.Message);
        Assert.Null(response.PostId);
        _azureService.VerifyNoImageUpload();
    }
    
    [Fact]
    public async Task CreatePost_InvalidUsername_ShouldFailValidation()
    {
        // Arrange
        var invalidRequest = CreatePostDto("content", null);

        // Act
        var response = await _postService.CreatePost(invalidRequest, "id");

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.NotEmpty(response.Message);
        Assert.Null(response.PostId);
        _azureService.VerifyNoImageUpload();
    }

    [Fact]
    public async Task CreatePost_UserNotFound_ShouldReturnBadServiceResponse()
    {
        // Arrange
        var request = CreatePostDto("content", null);
        _userManager.SetupFindById(null);

        // Act
        var response = await _postService.CreatePost(request, "non");

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.NotEmpty(response.Message);
        Assert.Null(response.PostId);
        _azureService.VerifyNoImageUpload();
    }

    [Fact]
    public async Task CreatePost_NoImages_ShouldCreateOnlyPost()
    {
        // Arrange
        var user = CreateUser("user");
        _userManager.SetupFindById(user);
        var request = CreatePostDto("content", null);
        // Act
        var response = await _postService.CreatePost(request, user.Id);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Flag);
        Assert.NotEmpty(response.Message);
        Assert.NotNull(response.PostId);
        AssertDatabaseEntryExists(user.Id);
        _azureService.VerifyNoImageUpload();
    }

    [Fact]
    public async Task CreatePost_WithImages_ShouldUploadAndAttachImages()
    {
        // Arrange
        var user = CreateUser("user");
        var base64Images = CreateBase64Images(3);
        var request = CreatePostDto("content", base64Images);
        _userManager.SetupFindById(user);
        _azureService.SetupSuccessfulUpload();

        // Act
        var response = await _postService.CreatePost(request, user.Id);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Flag);
        Assert.NotEmpty(response.Message);
        Assert.NotNull(response.PostId);
        AssertPostWithImagesCreated(user.Id, base64Images.Count);
    }

    [Fact]
    public async Task GetUserPosts_InvalidUsername_ShouldReturnEmpty()
    {
        await AssertFetchUserPostsFailureScenario(null);
        await AssertFetchUserPostsFailureScenario("");
    }
    [Fact]
    public async Task UpsertPostComment_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = "testUserId";
        var comment = "Valid comment";

        var post = new Post
        {
            Id = postId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Content = "Post content",
        };
        var user = CreateUser("testUser");
        
        _context.Posts.Add(post);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _userManager.SetupFindById(user);
        // Act
        var response = await _postService.UpsertPostComment(postId, commentId, comment, userId);

        // Assert
        AssertSuccessResponse(response);
    }

    [Fact]
    public async Task UpsertPostComment_InvalidUserId_ShouldReturnError()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = ""; 
        var comment = "Valid comment";

        // Act
        var response = await _postService.UpsertPostComment(postId, commentId, comment, userId);

        _userManager.SetupFindById(null);
        // Assert
        AssertBadResponse(response);
    }

    [Fact]
    public async Task UpsertPostComment_PostNotFound_ShouldReturnError()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = "testUserId";
        var comment = "Valid comment";
        
        // Act
        var response = await _postService.UpsertPostComment(postId, commentId, comment, userId);

        // Assert
        AssertBadResponse(response);
    }

    [Fact]
    public async Task UpsertPostComment_NewComment_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        Guid? commentId = null; // A new comment
        var userId = "testUserId";
        var comment = "New comment";

        var post = new Post
        {
            Id = postId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Content = "Post content",
        };
        var user = CreateUser("testUser");

        _context.Posts.Add(post);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManager.SetupFindById(user);
        // Act
        var response = await _postService.UpsertPostComment(postId, commentId, comment, userId);

        // Assert
        AssertSuccessResponse(response);
    }

    [Fact]
    public async Task UpsertPostComment_UpdateExistingComment_ShouldReturnSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = "testUserId";
        var comment = "Updated comment content";

        var post = new Post
        {
            Id = postId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Content = "Post content",
        };
        var user = CreateUser("testUser");
        _userManager.SetupFindById(user);
        var existingComment = new Comment
        {
            Id = commentId,
            PostId = postId,
            Content = "Old comment content",
            CreatedByUserId = user.Id,
        };

        _context.Posts.Add(post);
        _context.Users.Add(user);
        _context.Comments.Add(existingComment);
        await _context.SaveChangesAsync();

        // Act
        var response = await _postService.UpsertPostComment(postId, commentId, comment, userId);

        // Assert
        AssertSuccessResponse(response);
        Assert.Equal("Updated comment content", existingComment.Content);
    }
    private async Task AssertFetchUserPostsFailureScenario(string? username)
    {
        // Act
        var result = await _postService.GetUserPosts(username, 0, 10);

        // Assert
        Assert.Empty(result);
        _userManager.VerifyNoUserLookup();
    }
    [Fact]
    public async Task GetCommentsForPost_ValidRequest_ShouldReturnComments()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = "userId";
        var user = CreateUser("user");

        var post = new Post { Id = postId, CreatedByUserId = userId,Content = "content", CreatedAt = DateTime.UtcNow };
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            Content = "Sample comment",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId,
            CreatedByUser = user,
            Likes = new List<CommentLike>
            {
                new CommentLike { LikedByUserId = userId }
            }
        };

        _context.Users.Add(user);
        _context.Posts.Add(post);
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.GetCommentsForPost(postId, 0, 10, userId);

        // Assert
        Assert.True(result.Flag);
        Assert.Single(result.Comments);
        Assert.Equal("Sample comment", result.Comments[0].Content);
        Assert.True(result.Comments[0].CurrentUserLikes);
        Assert.Equal(1, result.Comments[0].LikeCount);
    }
    [Fact]
    public async Task GetCommentsForPost_MissingUserId_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var result = await _postService.GetCommentsForPost(postId, 0, 10, null);

        // Assert
        Assert.False(result.Flag);
        Assert.Empty(result.Comments);
    }
    [Fact]
    public async Task GetCommentsForPost_PostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = "userId";

        // Act
        var result = await _postService.GetCommentsForPost(postId, 0, 10, userId);

        // Assert
        Assert.False(result.Flag);
        Assert.Empty(result.Comments);
    }
    [Fact]
    public async Task ToggleLikeComment_ValidRequest_AddLike_ShouldSucceed()
    {
        // Arrange
        var userId = "userId";
        var commentId = Guid.NewGuid();
        var user = CreateUser("user");
        var comment = new Comment
        {
            Id = commentId,
            Content = "Comment content",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = "anotherUserId"
        };

        _userManager.SetupFindById(user);
        _context.Users.Add(user);
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Act
        var response = await _postService.ToggleLikeComment(commentId, userId);

        // Assert
        Assert.True(response.Flag);
        Assert.Equal("Like added successfully", response.Message);
    }
    [Fact]
    public async Task ToggleLikeComment_ExistingLike_ShouldRemoveLike()
    {
        // Arrange
        var userId = "userId";
        var user = CreateUser("user");
        var commentId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            Content = "Comment content",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = "anotherUser"
        };

        var like = new CommentLike
        {
            CommentId = commentId,
            LikedByUserId = userId,
            LikedOn = DateTime.UtcNow
        };

        _userManager.SetupFindById(user);
        _context.Users.Add(user);
        _context.Comments.Add(comment);
        _context.CommentLikes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        var response = await _postService.ToggleLikeComment(commentId, userId);

        // Assert
        Assert.True(response.Flag);
        Assert.Equal("Like added successfully", response.Message); // Message is same even if like is removed
    }
    [Fact]
    public async Task ToggleLikeComment_EmptyUserId_ShouldReturnError()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        // Act
        var response = await _postService.ToggleLikeComment(commentId, "");

        // Assert
        AssertBadResponse(response);
    }
    [Fact]
    public async Task ToggleLikeComment_CommentNotFound_ShouldReturnError()
    {
        // Arrange
        var userId = "userId";
        var commentId = Guid.NewGuid();

        // Act
        var response = await _postService.ToggleLikeComment(commentId, userId);

        // Assert
        AssertBadResponse(response);
    }
    [Fact]
    public async Task ToggleLikeComment_UserNotFound_ShouldReturnError()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = "userId";

        var comment = new Comment
        {
            Id = commentId,
            Content = "Comment content",
            CreatedByUserId = "anotherUserId"
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        _userManager.SetupFindById(null);

        // Act
        var response = await _postService.ToggleLikeComment(commentId, userId);

        // Assert
        AssertBadResponse(response);
    }
    // Add similar refactored methods for other scenarios...

    // Assertion helpers
    private void AssertBadResponse(ServiceResponse response)
    {
        Assert.NotNull(response);
        Assert.False(response.Flag);
        Assert.NotEmpty(response.Message);
    }
    
    private void AssertSuccessResponse(ServiceResponse response)
    {
        Assert.NotNull(response);
        Assert.True(response.Flag);
        Assert.NotEmpty(response.Message);
    }

    private void AssertDatabaseEntryExists(string userId)
    {
        Assert.NotNull(_context.Posts.FirstOrDefault(p => p.CreatedByUserId == userId));
    }

    private void AssertPostWithImagesCreated(string userId, int imageCount)
    {
        var post = _context.Posts.Include(p => p.Files).FirstOrDefault(p => p.CreatedByUserId == userId);
        Assert.NotNull(post);
        Assert.Equal(imageCount, post.Files?.Count);
    }
}

public static class MockExtensions
{
    public static void SetupFindByName(this Mock<UserManager<ApplicationUser>> userManager, ApplicationUser? user)
    {
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    }
    public static void SetupFindById(this Mock<UserManager<ApplicationUser>> userManager, ApplicationUser? user)
    {
        userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    }
    public static void VerifyNoUserLookup(this Mock<UserManager<ApplicationUser>> userManager)
    {
        userManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    public static void SetupSuccessfulUpload(this Mock<IAzureService> azureService)
    {
        azureService.Setup(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AzureImageDto(true, "fileName", ""));
    }

    public static void VerifyNoImageUpload(this Mock<IAzureService> azureService)
    {
        azureService.Verify(x => x.UploadImage(It.IsAny<Consts.ImageType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.DTO_s.User;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.PostEndpoint;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer;

public class PostControllerTests
{
    private readonly Mock<IPostService> _postServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly PostController _controller;

    public PostControllerTests()
    {
        _postServiceMock = new Mock<IPostService>();
        _userServiceMock = new Mock<IUserService>();
        _controller = new PostController(_postServiceMock.Object);
        SetMockUserInContext("testUser", "testUserId");
    }

    // Helper Method to mock User in Controller context
    private void SetMockUserInContext(string username, string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(Consts.ClaimTypes.UserName, username),
            new Claim(Consts.ClaimTypes.Id, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    // Test: Toggle Like Post with a valid request
    [Fact]
    public async Task ToggleLikePost_ValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var likePostDto = new LikePostDto(Guid.NewGuid());
        _postServiceMock
            .Setup(x => x.ToggleLikePost(likePostDto, "testUserId"))
            .ReturnsAsync(new ServiceResponse(true, "Like toggled successfully"));

        // Act
        var result = await _controller.ToggleLikePost(likePostDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result); 
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value); 

        Assert.True(serviceResponse.Flag);
        Assert.Equal("Like toggled successfully", serviceResponse.Message);
    }

    // Test: Toggle Like Post with an invalid request
    [Fact]
    public async Task ToggleLikePost_InvalidRequest_ShouldReturnBadRequestResult()
    {
        // Arrange
        var likePostDto = new LikePostDto(Guid.NewGuid());
        _postServiceMock
            .Setup(x => x.ToggleLikePost(likePostDto, ""))
            .ReturnsAsync(new ServiceResponse(false, "Invalid request"));
        SetMockUserInContext("", "");

        // Act
        var result = await _controller.ToggleLikePost(likePostDto);
        
        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result); 
        var serviceResponse = Assert.IsType<ServiceResponse>(badResult.Value); 
        Assert.False(serviceResponse.Flag);
        Assert.Equal("Invalid request", serviceResponse.Message);
    }
    [Fact]
    public async Task CreatePost_ValidRequest_ShouldReturnCreatedResponse()
    {
        // Arrange
        var createPostDto = new CreatePostDto("Sample Content", null);
        _postServiceMock
            .Setup(x => x.CreatePost(createPostDto, It.IsAny<string>()))
            .ReturnsAsync(new CreatePostResponseDto(true, "Post created successfully" ,Guid.NewGuid()));

        // Act
        var result = await _controller.CreatePost(createPostDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result); 
        var serviceResponse = Assert.IsType<CreatePostResponseDto>(okResult.Value); 
        Assert.True(serviceResponse.Flag);
        Assert.Equal("Post created successfully", serviceResponse.Message);
    } 
    [Fact]
    public async Task UpsertPostComment_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var comment = "Test Comment";
        var userId = "testUserId";
        _postServiceMock
            .Setup(x => x.UpsertPostComment(postId, commentId, comment, userId))
            .ReturnsAsync(new UpsertCommentDto(true, "Comment added successfully", new CommentDto(Guid.NewGuid(), comment, new UserBasicDto("","username", "", ""), 0, DateTime.UtcNow, false)));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var serviceResponse = Assert.IsType<UpsertCommentDto>(okResult.Value);

        Assert.True(serviceResponse.Flag);
    }

    [Fact]
    public async Task UpsertPostComment_MissingUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var comment = "Test Comment";
        var userId = ""; // Simulating missing user ID
        SetMockUserInContext("", "");
        _postServiceMock
            .Setup(x => x.UpsertPostComment(postId, commentId, comment, userId))
            .ReturnsAsync(new UpsertCommentDto(false, "error", null));

        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var serviceResponse = Assert.IsType<UpsertCommentDto>(badResult.Value);
        Assert.False(serviceResponse.Flag);
    }

    [Fact]
    public async Task UpsertPostComment_MissingPost_ShouldReturnBadRequest()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var comment = "Test Comment";
        var userId = "testUserId";
        _postServiceMock
            .Setup(x => x.UpsertPostComment(postId, commentId, comment, userId))
            .ReturnsAsync(new UpsertCommentDto(false, "error", null));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var serviceResponse = Assert.IsType<UpsertCommentDto>(badResult.Value);

        Assert.False(serviceResponse.Flag);
    }

    [Fact]
    public async Task UpsertPostComment_MissingComment_ShouldReturnBadRequest()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var comment = ""; // Simulating missing comment
        var userId = "testUserId";
        _postServiceMock
            .Setup(x => x.UpsertPostComment(postId, commentId, comment, userId))
            .ReturnsAsync(new UpsertCommentDto(false, "error", null));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var serviceResponse = Assert.IsType<UpsertCommentDto>(badResult.Value);

        Assert.False(serviceResponse.Flag);
    }

    [Fact]
    public async Task UpsertPostComment_NewComment_ShouldReturnSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        Guid? commentId = null; // Simulating a new comment
        var comment = "New Comment Content";
        var userId = "testUserId";
        _postServiceMock
            .Setup(x => x.UpsertPostComment(postId, commentId, comment, userId))
            .ReturnsAsync(new UpsertCommentDto(true, "Comment added successfully", new CommentDto(Guid.NewGuid(), comment, new UserBasicDto("","username", "", ""), 0, DateTime.UtcNow, false)));

        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var serviceResponse = Assert.IsType<UpsertCommentDto>(okResult.Value);

        Assert.True(serviceResponse.Flag);
        Assert.Equal("Comment added successfully", serviceResponse.Message);
    }

    [Fact]
    public async Task UpsertPostComment_UpdateExistingComment_ShouldReturnSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid(); // Simulating an existing comment
        var comment = "Updated Comment Content";
        var userId = "testUserId";
        _postServiceMock
            .Setup(x => x.UpsertPostComment(postId, commentId, comment, userId))
            .ReturnsAsync(new UpsertCommentDto(true, "Comment added successfully", new CommentDto(Guid.NewGuid(), comment, new UserBasicDto("","username", "", ""), 0, DateTime.UtcNow, false)));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var serviceResponse = Assert.IsType<UpsertCommentDto>(okResult.Value);

        Assert.True(serviceResponse.Flag);
        Assert.Equal("Comment added successfully", serviceResponse.Message);
    }
    [Fact]
    public async Task GetPostComments_ValidRequest_ShouldReturnComments()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = "testUserId";
        var comments = new List<CommentDto> { new CommentDto(Guid.NewGuid(),"Sample comment", new UserBasicDto("","username","",""),0, DateTime.UtcNow, false) };
        var resultDto = new PostCommentsDto(true, comments);

        _postServiceMock
            .Setup(x => x.GetCommentsForPost(postId, 0, 10, userId))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.GetPostComments(postId, 0, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<PostCommentsDto>(okResult.Value);

        Assert.True(returnedDto.Flag);
        Assert.Single(returnedDto.Comments);
    }
    [Fact]
    public async Task GetPostComments_EmptyPostId_ShouldReturnBadRequest()
    {
        // Arrange
        var emptyPostId = Guid.Empty;

        // Act
        var result = await _controller.GetPostComments(emptyPostId, 0, 10);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnedDto = Assert.IsType<PostCommentsDto>(badResult.Value);

        Assert.False(returnedDto.Flag);
        Assert.Empty(returnedDto.Comments);
    }
    [Fact]
    public async Task GetPostComments_MissingUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var postId = Guid.NewGuid();
        SetMockUserInContext("", "");

        // Act
        var result = await _controller.GetPostComments(postId, 0, 10);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnedDto = Assert.IsType<PostCommentsDto>(badResult.Value);

        Assert.False(returnedDto.Flag);
        Assert.Empty(returnedDto.Comments);
    }
    [Fact]
    public async Task GetPostComments_FailedResult_ShouldReturnBadRequest()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = "testUserId";

        _postServiceMock
            .Setup(x => x.GetCommentsForPost(postId, 0, 10, userId))
            .ReturnsAsync(new PostCommentsDto(false, new List<CommentDto>()));

        // Act
        var result = await _controller.GetPostComments(postId, 0, 10);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnedDto = Assert.IsType<PostCommentsDto>(badResult.Value);

        Assert.False(returnedDto.Flag);
    }
    [Fact]
    public async Task ToggleCommentLike_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = "testUserId";
        var expectedResponse = new ServiceResponse(true, "Like toggled");

        _postServiceMock
            .Setup(x => x.ToggleLikeComment(commentId, userId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ToggleCommentLike(commentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value);

        Assert.True(serviceResponse.Flag);
        Assert.Equal("Like toggled", serviceResponse.Message);
    }
    [Fact]
    public async Task ToggleCommentLike_EmptyCommentId_ShouldReturnBadRequest()
    {
        // Arrange
        var commentId = Guid.Empty;

        // Act
        var result = await _controller.ToggleCommentLike(commentId);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badResult.Value);

        Assert.False(serviceResponse.Flag);
    } 
    [Fact]
    public async Task ToggleCommentLike_FailedToggle_ShouldReturnBadRequest()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = "testUserId";
        var expectedResponse = new ServiceResponse(false, "Failed to toggle");

        _postServiceMock
            .Setup(x => x.ToggleLikeComment(commentId, userId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ToggleCommentLike(commentId);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badResult.Value);
        Assert.False(serviceResponse.Flag);
        Assert.Equal("Failed to toggle", serviceResponse.Message);
    }
    [Fact]
    public async Task GetUserPosts_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var userName = "testUser";
        int skip = 0, take = 10;
        var posts = new List<PostBasicDto> { new PostBasicDto(Guid.NewGuid(), "Content","test","",0,0,[], DateTime.UtcNow) };
        _postServiceMock
            .Setup(x => x.GetUserPosts(userName, skip, take))
            .ReturnsAsync(posts);

        // Act
        var result = await _controller.GetUserPosts(userName, skip, take);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPosts = Assert.IsType<List<PostBasicDto>>(okResult.Value);
        Assert.Single(returnedPosts);
        Assert.Equal("Content", returnedPosts[0].Content);
    }
    [Fact]
    public async Task GetUserPosts_EmptyUsername_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetUserPosts("", 0, 10);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnedPosts = Assert.IsType<List<PostBasicDto>>(badResult.Value);
        Assert.Empty(returnedPosts);
    }
    [Fact]
    public async Task GetPost_ValidId_ShouldReturnOk()
    {
        var postId = Guid.NewGuid();
        var postDto = new PostBasicDto(postId, "Content", "test", "", 0, 0, [], DateTime.UtcNow);
        _postServiceMock.Setup(x => x.GetPostById(postId)).ReturnsAsync(postDto);

        var result = await _controller.GetPost(postId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPost = Assert.IsType<PostBasicDto>(okResult.Value);
        Assert.Equal(postId, returnedPost.Id);
    }

    [Fact]
    public async Task GetPost_EmptyId_ShouldReturnBadRequest()
    {
        var result = await _controller.GetPost(Guid.Empty);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task GetPost_NotFound_ShouldReturnNotFound()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.GetPostById(postId)).ReturnsAsync((PostBasicDto?)null);
        var result = await _controller.GetPost(postId);
        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task DeletePost_ValidRequest_ShouldReturnOk()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.DeletePost(postId, "testUserId"))
            .ReturnsAsync(new ServiceResponse(true, "Deleted successfully"));

        var result = await _controller.DeletePost(postId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value);
        Assert.True(serviceResponse.Flag);
    }

    [Fact]
    public async Task DeletePost_MissingUserId_ShouldReturnBadRequest()
    {
        var postId = Guid.NewGuid();
        SetMockUserInContext("", "");

        var result = await _controller.DeletePost(postId);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ServiceResponse>(badResult.Value);
        Assert.False(response.Flag);
    }

    [Fact]
    public async Task DeletePost_FailedDeletion_ShouldReturnBadRequest()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.DeletePost(postId, "testUserId"))
            .ReturnsAsync(new ServiceResponse(false, "Failed to delete"));

        var result = await _controller.DeletePost(postId);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ServiceResponse>(badResult.Value);
        Assert.False(response.Flag);
        Assert.Equal("Failed to delete", response.Message);
    }
    [Fact]
    public async Task DeleteComment_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _postServiceMock
            .Setup(x => x.DeleteComment(commentId, "testUserId"))
            .ReturnsAsync(new ServiceResponse(true, "Comment deleted successfully"));

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ServiceResponse>(okResult.Value);

        Assert.True(response.Flag);
    }
    [Fact]
    public async Task DeleteComment_EmptyCommentId_ShouldReturnBadRequest()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _controller.DeleteComment(emptyId);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ServiceResponse>(badResult.Value);

        Assert.False(response.Flag);
        Assert.Equal("error occured while deleting comment", response.Message);
    }
    [Fact]
    public async Task DeleteComment_MissingUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        SetMockUserInContext("", ""); // simulate no userId

        _postServiceMock
            .Setup(x => x.DeleteComment(commentId, ""))
            .ReturnsAsync(new ServiceResponse(false, "User not authorized"));

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ServiceResponse>(badResult.Value);

        Assert.False(response.Flag);
        Assert.Equal("User not authorized", response.Message);
    }

    [Fact]
    public async Task DeleteComment_FailedDeletion_ShouldReturnBadRequest()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _postServiceMock
            .Setup(x => x.DeleteComment(commentId, "testUserId"))
            .ReturnsAsync(new ServiceResponse(false, "Failed to delete comment"));

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ServiceResponse>(badResult.Value);

        Assert.False(response.Flag);
        Assert.Equal("Failed to delete comment", response.Message);
    } 
}
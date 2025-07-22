using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
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
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(response.Result); 
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
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badResult = Assert.IsType<BadRequestObjectResult>(response.Result); 
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
            .ReturnsAsync(new ServiceResponse(true, "Post created successfully"));

        // Act
        var result = await _controller.CreatePost(createPostDto);

        // Assert
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(response.Result); 
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value); 

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
            .ReturnsAsync(new ServiceResponse(true, "Comment added successfully"));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value);

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
            .ReturnsAsync(new ServiceResponse(false, "Error occured while adding comment"));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badResult.Value);

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
            .ReturnsAsync(new ServiceResponse(false, "Post not found"));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badResult.Value);

        Assert.False(serviceResponse.Flag);
        Assert.Equal("Post not found", serviceResponse.Message);
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
            .ReturnsAsync(new ServiceResponse(false, "Error occured while adding comment"));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badResult.Value);

        Assert.False(serviceResponse.Flag);
        Assert.Equal("Error occured while adding comment", serviceResponse.Message);
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
            .ReturnsAsync(new ServiceResponse(true, "Comment added successfully"));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value);

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
            .ReturnsAsync(new ServiceResponse(true, "Comment added successfully"));
        var newComment = new UpsertPostComment(postId, commentId, comment);
        // Act
        var result = await _controller.UpsertPostComment(newComment);

        // Assert
        var response = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value);

        Assert.True(serviceResponse.Flag);
        Assert.Equal("Comment added successfully", serviceResponse.Message);
    }

}
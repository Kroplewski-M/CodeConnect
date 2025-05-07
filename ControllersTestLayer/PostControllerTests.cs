using System.Security.Claims;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using CodeConnect.WebAPI.Endpoints.PostEndpoint;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer;

public class PostControllerTests
{
    private readonly Mock<IPostService> _mockPostService;
    private readonly PostController _postController;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    public PostControllerTests()
    {
        _mockPostService = new Mock<IPostService>();
        _postController = new PostController(_mockPostService.Object);
        _userManagerMock = new Mock<UserManager<ApplicationUser>>();
        var currentUsername = "testUsername";
        var claims = new[] { new Claim(Consts.ClaimTypes.UserName, currentUsername) };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _postController.ControllerContext = new ControllerContext(){ HttpContext = httpContext };
    }

    [Fact]
    public async Task CreatePost_NoContent_ShouldReturnBadRequest()
    {
        //Arrange
        var createPostDto = new CreatePostDto("",new List<Base64Dto>(),"testUsername");
        
        //Act
        var result = await _postController.CreatePost(createPostDto);
 
        //Assert
        var actionResult = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badRequestResult.Value);
        Assert.False(serviceResponse.Flag); 
        _mockPostService.Verify(x => x.CreatePost(It.IsAny<CreatePostDto>()), Times.Never);
    }
    [Fact]
    public async Task CreatePost_NoUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var createPostDto = new CreatePostDto("SomeContent",new List<Base64Dto>(),"");
        
        //Act
        var result = await _postController.CreatePost(createPostDto);
 
        //Assert
        var actionResult = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badRequestResult.Value);
        Assert.False(serviceResponse.Flag); 
        _mockPostService.Verify(x => x.CreatePost(It.IsAny<CreatePostDto>()), Times.Never);
    }
    [Fact]
    public async Task CreatePost_NoUsernameOrContent_ShouldReturnBadRequest()
    {
        //Arrange
        var createPostDto = new CreatePostDto("",new List<Base64Dto>(),"");
        
        //Act
        var result = await _postController.CreatePost(createPostDto);
 
        //Assert
        var actionResult = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badRequestResult.Value);
        Assert.False(serviceResponse.Flag); 
        _mockPostService.Verify(x => x.CreatePost(It.IsAny<CreatePostDto>()), Times.Never);
    }
    [Fact]
    public async Task CreatePost_WrongUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var createPostDto = new CreatePostDto("some content",new List<Base64Dto>(),"wrongUsername");
        
        //Act
        var result = await _postController.CreatePost(createPostDto);
 
        //Assert
        var actionResult = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(badRequestResult.Value);
        Assert.False(serviceResponse.Flag); 
        _mockPostService.Verify(x => x.CreatePost(It.IsAny<CreatePostDto>()), Times.Never);
    }
    [Fact]
    public async Task CreatePost_ValidDto_ShouldReturnOk()
    {
        //Arrange
        var createPostDto = new CreatePostDto("some content",new List<Base64Dto>(),"testUsername");
        var expectedResponse = new ServiceResponse(true, "Post created successfully");
        _mockPostService.Setup(x => x.CreatePost(createPostDto)).ReturnsAsync(expectedResponse);
        
        //Act
        var result = await _postController.CreatePost(createPostDto);
 
        //Assert
        var actionResult = Assert.IsType<ActionResult<ServiceResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var serviceResponse = Assert.IsType<ServiceResponse>(okResult.Value);
        Assert.True(serviceResponse.Flag); 
        _mockPostService.Verify(x => x.CreatePost(createPostDto), Times.Once);
    }

    [Fact]
    public async Task GetUsersPosts_EmptyUsername_ShouldReturnBadRequest()
    {
        //Arrange
        var username = "";
        //Act
        var result = await _postController.GetUserPosts(username);
        
        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PostBasicDto>>>(result);
        var badResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var returnedPosts = Assert.IsType<List<PostBasicDto>>(badResult.Value);
        Assert.Empty(returnedPosts);
        _mockPostService.Verify(x => x.GetUserPosts(username), Times.Never);
    }
    [Fact]
    public async Task GetUsersPosts_CorrectUsername_ShouldReturnPosts()
    {
        // Arrange
        var username = "testUsername";
        var expectedPosts = new List<PostBasicDto>
        {
            new PostBasicDto(1, "", "","", 0, 0, [], DateTime.Now),
            new PostBasicDto(2, "", "", "",0, 0, [], DateTime.Now),
        };

        _mockPostService.Setup(x => x.GetUserPosts(username)).ReturnsAsync(expectedPosts);
        _userManagerMock.Setup(x => x.FindByNameAsync(username)).ReturnsAsync(new ApplicationUser());
        // Act
        var result = await _postController.GetUserPosts(username);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PostBasicDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPosts = Assert.IsType<List<PostBasicDto>>(okResult.Value);
        Assert.Equal(2, returnedPosts.Count);
        _mockPostService.Verify(x => x.GetUserPosts(username), Times.Once);
    }
    
}
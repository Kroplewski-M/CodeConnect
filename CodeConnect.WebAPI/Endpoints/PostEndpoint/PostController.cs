using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.PostEndpoint;
[Route("api/[controller]")]
[ApiController]
public class PostController(IPostService postService) : ControllerBase
{
    [HttpPost("CreatePost")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<ActionResult<ServiceResponse>> CreatePost([FromBody]CreatePostDto createPost)
    {
        var postValidator = new CreatePostDtoValidator();
        var validate = await postValidator.ValidateAsync(createPost);
        if(!validate.IsValid)
            return  BadRequest(new ServiceResponse(false, "Error Creating Post"));

        var response = await postService.CreatePost(createPost, User.GetInfo(Consts.ClaimTypes.Id));
        if(!response.Flag)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("GetUserPosts")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<ActionResult<List<PostBasicDto>>> GetUserPosts(string userName, int skip, int take)
    {
        if(string.IsNullOrEmpty(userName))
            return BadRequest(new List<PostBasicDto>());
        return Ok(await postService.GetUserPosts(userName, skip, take));
    }

    [HttpGet("GetPost")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<ActionResult<PostBasicDto?>> GetPost(Guid id)
    {
        if(id == Guid.Empty)
            return BadRequest();
        var post = await postService.GetPostById(id);
        if(post == null)
            return NotFound();
        return Ok(post);
    }

    [HttpPost("ToggleLikePost")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<ActionResult<ServiceResponse>> ToggleLikePost(LikePostDto likePostDto)
    {
        var result = await postService.ToggleLikePost(likePostDto, User.GetInfo(Consts.ClaimTypes.Id));
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("IsUserLikingPost")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<ActionResult<bool>> IsUserLikingPost(Guid postId)
    {
        var result = await postService.IsUserLikingPost(postId, User.GetInfo(Consts.ClaimTypes.Id));
        return Ok(result);
    }
    // [HttpPut("UpdatePost")]
    // [Authorize]
    // public async Task<IActionResult> UpdatePost()
    // {
    //     return null;
    // }
    //
    // [HttpDelete("DeletePost")]
    // [Authorize]
    // public async Task<IActionResult> DeletePost()
    // {
    //     return null;
    // }
    [HttpPost("UpsertPostComment")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<ServiceResponse> UpsertPostComment(UpsertPostComment postComment)
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return new ServiceResponse(false, "User not found");
        var result = await postService.AddPostComment(postComment.PostId, postComment.Content, userId);
        return result;
    }
}
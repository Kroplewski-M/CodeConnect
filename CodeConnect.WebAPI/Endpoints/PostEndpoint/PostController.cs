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
    public async Task<IActionResult> CreatePost([FromBody]CreatePostDto createPost)
    {
        var postValidator = new CreatePostDtoValidator();
        var validate = await postValidator.ValidateAsync(createPost);
        if(!validate.IsValid)
            return  BadRequest(new CreatePostResponseDto(false, "Error Creating Post", null));

        var response = await postService.CreatePost(createPost, User.GetInfo(Consts.ClaimTypes.Id));
        if(!response.Flag)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("GetUserPosts")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<IActionResult> GetUserPosts(string userName, int skip, int take)
    {
        if(string.IsNullOrEmpty(userName))
            return BadRequest(new List<PostBasicDto>());
        return Ok(await postService.GetUserPosts(userName, skip, take));
    }

    [HttpGet("GetPost")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<IActionResult> GetPost(Guid id)
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
    public async Task<IActionResult> ToggleLikePost(LikePostDto likePostDto)
    {
        var result = await postService.ToggleLikePost(likePostDto, User.GetInfo(Consts.ClaimTypes.Id));
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("IsUserLikingPost")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<IActionResult> IsUserLikingPost(Guid postId)
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
    public async Task<IActionResult> UpsertPostComment(UpsertPostComment postComment)
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest(new ServiceResponse(false, "User not found"));
        var result = await postService.UpsertPostComment(postComment.PostId,postComment.CommentId, postComment.Content, userId);
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("GetPostComments")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<IActionResult> GetPostComments(Guid postId, int skip, int take)
    {
        if(postId == Guid.Empty)
            return BadRequest(new PostCommentsDto(false, new List<CommentDto>()));
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest(new PostCommentsDto(false, new List<CommentDto>()));
        var result = await postService.GetCommentsForPost(postId, skip, take, userId);
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("ToggleCommentLike")]
    [Authorize(nameof(Consts.TokenType.Access))]
    public async Task<IActionResult> ToggleCommentLike([FromBody] Guid commentId)
    {
       var userId = User.GetInfo(Consts.ClaimTypes.Id);
       if(commentId == Guid.Empty)
           return BadRequest(new ServiceResponse(false, "Comment Id not found"));
       var res = await postService.ToggleLikeComment(commentId, userId);
       if(res.Flag)
        return Ok(res);
       return BadRequest(res);
    }
}
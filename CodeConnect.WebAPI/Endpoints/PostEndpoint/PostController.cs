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
[Authorize(nameof(Consts.TokenType.Access))]
public class PostController(IPostService postService) : ControllerBase
{
    [HttpPost("CreatePost")]
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
    public async Task<IActionResult> GetUserPosts(string userName, int skip, int take)
    {
        if(string.IsNullOrEmpty(userName))
            return BadRequest(new List<PostDto>());
        return Ok(await postService.GetUserPosts(userName, skip, take));
    }

    [HttpGet("GetPostsForFeed")]
    public async Task<IActionResult> GetPostsForFeed(int skip, int take)
    {
        return Ok(await postService.GetPostsForFeed(skip, take, User.GetInfo(Consts.ClaimTypes.Id)));
    }
    [HttpGet("GetPost")]
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
    public async Task<IActionResult> ToggleLikePost(LikePostDto likePostDto)
    {
        var result = await postService.ToggleLikePost(likePostDto, User.GetInfo(Consts.ClaimTypes.Id));
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("IsUserLikingPost")]
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
     [HttpDelete("DeletePost")]
     public async Task<IActionResult> DeletePost(Guid postId)
     {
         var userId =  User.GetInfo(Consts.ClaimTypes.Id);
         if(string.IsNullOrWhiteSpace(userId))
             return BadRequest(new ServiceResponse(false, "User not found"));
         var result = await postService.DeletePost(postId, userId);
         if(result.Flag)
             return Ok(result);
         return BadRequest(result);
     }
    [HttpPost("UpsertPostComment")]
    public async Task<IActionResult> UpsertPostComment(UpsertPostComment postComment)
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(string.IsNullOrWhiteSpace(userId))
            return BadRequest(new UpsertCommentDto(false, "User not found", null));
        var result = await postService.UpsertPostComment(postComment.PostId,postComment.CommentId, postComment.Content, userId);
        if(result.Flag)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("GetPostComments")]
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
    public async Task<IActionResult> ToggleCommentLike([FromBody] Guid commentId)
    {
       var userId = User.GetInfo(Consts.ClaimTypes.Id);
       if(commentId == Guid.Empty)
           return BadRequest(new ServiceResponse(false, "error occured while adding comment"));
       var res = await postService.ToggleLikeComment(commentId, userId);
       if(res.Flag)
        return Ok(res);
       return BadRequest(res);
    }
    [HttpDelete("DeleteComment")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = User.GetInfo(Consts.ClaimTypes.Id);
        if(commentId == Guid.Empty)
            return BadRequest(new ServiceResponse(false, "error occured while deleting comment"));
        var res = await postService.DeleteComment(commentId, userId);
        if(res.Flag)
            return Ok(res);
        return BadRequest(res);
    }
}
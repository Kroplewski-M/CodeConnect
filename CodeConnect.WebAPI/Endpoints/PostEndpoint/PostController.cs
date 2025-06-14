using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
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
    [Authorize]
    public async Task<ActionResult<ServiceResponse>> CreatePost([FromBody]CreatePostDto createPost)
    {
        var postValidator = new CreatePostDtoValidator();
        var validate = await postValidator.ValidateAsync(createPost);
        if(!validate.IsValid || User.FindFirst(Consts.ClaimTypes.UserName)?.Value != createPost.CreatedByUserName)
            return  BadRequest(new ServiceResponse(false, "Error Creating Post"));

        var response = await postService.CreatePost(createPost);
        if(!response.Flag)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("GetUserPosts")]
    [Authorize]
    public async Task<ActionResult<List<PostBasicDto>>> GetUserPosts(string userName, int skip, int take)
    {
        if(string.IsNullOrEmpty(userName))
            return BadRequest(new List<PostBasicDto>());
        return Ok(await postService.GetUserPosts(userName, skip, take));
    }

    [HttpGet("GetPost")]
    [Authorize]
    public async Task<ActionResult<PostDto?>> GetPost(Guid id)
    {
        var post = await postService.GetPostById(id);
        if(post == null)
            return NotFound();
        return Ok(post);
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
}
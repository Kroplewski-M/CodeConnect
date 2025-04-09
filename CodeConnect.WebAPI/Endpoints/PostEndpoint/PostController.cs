using ApplicationLayer.DTO_s;
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
    public async Task<ServiceResponse> CreatePost([FromBody]CreatePostDto createPost)
    {
        var postValidator = new CreatePostDtoValidator();
        var validate = await postValidator.ValidateAsync(createPost);
        if(!validate.IsValid)
            return new ServiceResponse(false, "Error Creating Post");
        if(User.FindFirst(Consts.ClaimTypes.UserName)?.Value != createPost.CreatedByUserName)
            return new ServiceResponse(false, "Error Creating Post");
        
        var response = await postService.CreatePost(createPost);
        return response;
    }

    [HttpGet("GetPosts")]
    [Authorize]
    public List<PostDto> GetPosts()
    {
        
        return new List<PostDto>();
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
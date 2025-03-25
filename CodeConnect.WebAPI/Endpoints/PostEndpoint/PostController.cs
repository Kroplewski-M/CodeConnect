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
    public async Task<ServiceResponse> CreatePost([FromBody]PostDto post)
    {
        if(User.FindFirst(Consts.ClaimTypes.UserName)?.Value != post.CreatedByUser)
            return new ServiceResponse(false, "Error Creating Post");
        var response = await postService.CreatePost(post);
        return response;
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
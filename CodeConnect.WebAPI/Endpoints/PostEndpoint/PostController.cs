using ApplicationLayer.DTO_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CodeConnect.WebAPI.Endpoints.PostEndpoint;
[Route("api/[controller]")]
[ApiController]
public class PostController
{
    [HttpPost("CreatePost")]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody]PostDTO post)
    {
        return null;
    }

    [HttpPut("UpdatePost")]
    [Authorize]
    public async Task<IActionResult> UpdatePost()
    {
        return null;
    }

    [HttpDelete("DeletePost")]
    [Authorize]
    public async Task<IActionResult> DeletePost()
    {
        return null;
    }
}
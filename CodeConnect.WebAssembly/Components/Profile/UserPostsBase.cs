using ApplicationLayer.APIServices;
using ApplicationLayer.Classes;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Profile;

public class UserPostsBase : ComponentBase
{
    [Inject] public required IPostService PostService { get; set; }
    [CascadingParameter] public required string Username { get; set; }
    
    public List<PostBasicDto> Posts { get; set; } = new List<PostBasicDto>();
    public bool Loading { get; set; } = true;

    protected async Task LoadMorePosts((int,int)range)
    {
        var (startIndex, take) = range;
        var more = await PostService.GetUserPosts(Username, skip: startIndex, take: take);
        if (more?.Any() == true)
        {
            Posts.AddRange(more);
        }
    }
}
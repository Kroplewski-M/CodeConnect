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
    [CascadingParameter] public required UserState UserState { get; set; }
    
    public List<PostBasicDto> Posts { get; set; } = new List<PostBasicDto>();
    public bool Loading { get; set; } = true;

    protected async Task LoadMorePosts((int,int)range)
    {
        if(UserState.Current == null) return;
        var (startIndex, take) = range;
        var more = await PostService.GetUserPosts(UserState.Current.UserName, skip: startIndex, take: take);
        if (more?.Any() == true)
        {
            Posts.AddRange(more);
        }
    }
}
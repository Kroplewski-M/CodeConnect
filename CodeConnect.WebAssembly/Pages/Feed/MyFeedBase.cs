using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Pages.Feed;

public class MyFeedBase : ComponentBase
{
    [Inject] public required IPostService PostService { get; set; }
    protected List<PostDto> Posts { get; set; } = new List<PostDto>();
    public bool Loading { get; set; } = true;

    protected async Task LoadMorePosts((int,int)range)
    {
        var (startIndex, take) = range;
        var more = await PostService.GetPostsForFeed(skip: startIndex, take: take);
        if (more?.Any() == true)
        {
            Posts.AddRange(more);
        }
    }
}
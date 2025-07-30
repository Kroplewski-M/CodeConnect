using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CodeConnect.WebAssembly.Components.Posts;

public class PostCommentsBase : ComponentBase
{
    [CascadingParameter] public required Guid PostId { get; set; }
    [Inject] public required IPostService PostService { get; set; } 
    
    protected List<CommentDto> Comments = new List<CommentDto>();
    
    protected async Task LoadMoreComments((int,int)range)
    {
        var (startIndex, take) = range;
        var more = await PostService.GetCommentsForPost(PostId, skip: startIndex, take: take);
        if (more.Comments.Count != 0)
        {
            Comments.AddRange(more.Comments);
            StateHasChanged();
        }
    }
}
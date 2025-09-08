using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace CodeConnect.WebAssembly.Components.Posts;

public class PostCommentsBase : ComponentBase
{
    [CascadingParameter] public required Guid PostId { get; set; }
    [Inject] public required IPostService PostService { get; set; } 
    [Inject] public required NavigationManager NavigationManager { get; set; }
    
    protected List<CommentDto> Comments = new List<CommentDto>();
    protected Guid? HighlightCommentId { get; set; }
    protected override async Task OnInitializedAsync()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var queryDict = QueryHelpers.ParseQuery(uri.Query); 
        queryDict.TryGetValue("highlightComment", out var commentIdString);
        Guid.TryParse(commentIdString, out var commentId);
        if (commentId != Guid.Empty)
        {
            HighlightCommentId = commentId;
        }
        if (HighlightCommentId != null && !_fetchedHighlightComment)
        {
            var highlightResult = await PostService.GetCommentsForPost(
                PostId,
                skip: 0,
                take: 0,
                highlightCommentId: HighlightCommentId);

            if (highlightResult.Flag && highlightResult.Comments.Any())
            {
                var highlight = highlightResult.Comments.FirstOrDefault();
                if (highlight != null)
                    Comments.Insert(0, highlight);
                _fetchedHighlightComment = true;
            }
        }
    }
    protected void AddCommentToTop(CommentDto newComment)
    {
        Comments.Insert(0, newComment);
        StateHasChanged();
    }

    protected void DeleteComment(Guid commentId)
    {
        var comment = Comments.FirstOrDefault(x => x.Id == commentId);
        if (comment != null)
        {
            Comments.Remove(comment);
            StateHasChanged();   
        }
    }
    private bool _fetchedHighlightComment;
    protected async Task LoadMoreComments((int,int)range)
    {
        var (startIndex, take) = range;
        PostCommentsDto? moreComments = await PostService.GetCommentsForPost(PostId, skip: startIndex, take: take);
        if (moreComments.Comments.Count != 0)
        {
            if (HighlightCommentId != null)
            {
                moreComments.Comments.RemoveAll(x => x.Id == HighlightCommentId);
            }
            Comments.AddRange(moreComments.Comments);
            StateHasChanged();
        }
    }
}
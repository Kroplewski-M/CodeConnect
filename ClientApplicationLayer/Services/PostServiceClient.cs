using System.Net;
using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;

namespace ClientApplicationLayer.Services;

public class PostServiceClient(HttpClient httpClient) : IPostService
{
    public async Task<CreatePostResponseDto> CreatePost(CreatePostDto createPost, string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync("api/Post/CreatePost", createPost);
        var result = await response.Content.ReadFromJsonAsync<CreatePostResponseDto>();
        return result ?? new CreatePostResponseDto(false, "Failed to create post", null);
    }

    public async Task<PostBasicDto?> GetPostById(Guid id)
    {
        var response = await httpClient.GetAsync($"api/Post/GetPost?id={id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<PostBasicDto>();
    }

    public Task UpdatePost(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResponse> DeletePost(Guid id, string? userId = null)
    {
        var response = await  httpClient.DeleteAsync($"api/Post/DeletePost?postId={id}");
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        return result ?? new ServiceResponse(false, "Error deleting post");
    }
    public async Task<List<PostBasicDto>> GetUserPosts(string username, int skip, int take)
    {
        var response = await httpClient.GetFromJsonAsync<List<PostBasicDto>>($"api/Post/GetUserPosts?username={username}&Skip={skip}&Take={take}");
        return response ?? new List<PostBasicDto>();
    }

    public async Task<ServiceResponse> ToggleLikePost(LikePostDto likePostDto, string? userId = null)
    {
        var response = await httpClient.PostAsJsonAsync("api/Post/ToggleLikePost", likePostDto);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        return result ?? new ServiceResponse(false, "Failed to toggle like post");
    }

    public async Task<bool> IsUserLikingPost(Guid postId, string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<bool>($"api/Post/IsUserLikingPost?postId={postId}");
        return response;
    }

    public async Task<ServiceResponse> UpsertPostComment(Guid postId,Guid? commentId, string comment, string? userId = null)
    {
        var postComment = new UpsertPostComment(postId,commentId, comment);
        var response = await httpClient.PostAsJsonAsync("api/Post/UpsertPostComment", postComment);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        return result ?? new ServiceResponse(false, "Failed to toggle like post");
    }

    public async Task<PostCommentsDto> GetCommentsForPost(Guid postId, int skip, int take,  string? userId = null)
    {
        var response = await httpClient.GetFromJsonAsync<PostCommentsDto>($"api/Post/GetPostComments?postId={postId}&Skip={skip}&Take={take}");
        if (response == null)
            return new PostCommentsDto(false, new List<CommentDto>());
        return response;
    }

    public async Task<ServiceResponse> ToggleLikeComment(Guid commentId, string? userId = null)
    {
       var response = await httpClient.PostAsJsonAsync("api/Post/ToggleCommentLike", commentId);
       var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
       return result ?? new ServiceResponse(false, "Failed to toggle like comment");
    }
}
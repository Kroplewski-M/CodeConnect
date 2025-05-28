using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.ClientServices;

public class PostServiceClient(HttpClient httpClient) : IPostService
{
    public async Task<ServiceResponse> CreatePost(CreatePostDto createPost)
    {
        var response = await httpClient.PostAsJsonAsync("api/Post/CreatePost", createPost);
        var result = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        return result ?? new ServiceResponse(false, "Failed to create post");
    }

    public async Task<PostDto?> GetPostById(Guid id)
    {
        var response = await httpClient.GetFromJsonAsync<PostDto?>($"api/Post/GetPost?id={id}");
        return response;
    }

    public Task UpdatePost(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task DeletePost(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<PostBasicDto>> GetUserPosts(string username, int skip, int take)
    {
        var response = await httpClient.GetFromJsonAsync<List<PostBasicDto>>($"api/Post/GetUserPosts?username={username}&Skip={skip}&Take={take}");
        return response ?? new List<PostBasicDto>();
    }
}
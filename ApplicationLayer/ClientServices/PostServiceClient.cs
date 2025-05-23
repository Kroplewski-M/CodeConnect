using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
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

    public Task<Post> GetPostById(int id)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePost(int id)
    {
        throw new NotImplementedException();
    }

    public Task DeletePost(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<PostBasicDto>> GetUserPosts(string username)
    {
        var response = await httpClient.GetFromJsonAsync<List<PostBasicDto>>($"api/Post/GetUserPosts?username={username}");
        return response ?? new List<PostBasicDto>();
    }
}
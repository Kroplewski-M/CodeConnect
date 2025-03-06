using System.Net.Http.Json;
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.ClientServices;

public class PostServiceClient(HttpClient httpClient) : IPostService
{
    public async Task CreatePost(PostDTO post)
    {
        var response = await httpClient.PostAsJsonAsync("api/Post/CreatePost", post);
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
}
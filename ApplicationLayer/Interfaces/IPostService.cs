using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.Interfaces;

public interface IPostService
{
    public Task<ServiceResponse> CreatePost(PostDto post);
    public Task<Post> GetPostById(int id);
    public Task UpdatePost(int id);
    public Task DeletePost(int id);
}
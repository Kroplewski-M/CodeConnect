using ApplicationLayer.DTO_s;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.Interfaces;

public interface IPostService
{
    public Task<ServiceResponse> CreatePost(CreatePostDto createPost);
    public Task<Post> GetPostById(int id);
    public Task UpdatePost(int id);
    public Task DeletePost(int id);
    public Task<List<PostBasicDto>> GetUserPosts(string username);
}
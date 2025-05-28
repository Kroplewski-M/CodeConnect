using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.Interfaces;

public interface IPostService
{
    public Task<ServiceResponse> CreatePost(CreatePostDto createPost);
    public Task<PostDto?> GetPostById(int id);
    public Task UpdatePost(int id);
    public Task DeletePost(int id);
    public Task<List<PostBasicDto>> GetUserPosts(string username,  int skip, int take);
}
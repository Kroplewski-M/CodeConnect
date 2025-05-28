using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.Interfaces;

public interface IPostService
{
    public Task<ServiceResponse> CreatePost(CreatePostDto createPost);
    public Task<PostDto?> GetPostById(Guid id);
    public Task UpdatePost(Guid id);
    public Task DeletePost(Guid id);
    public Task<List<PostBasicDto>> GetUserPosts(string username,  int skip, int take);
}
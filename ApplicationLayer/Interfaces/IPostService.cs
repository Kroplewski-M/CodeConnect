using DomainLayer.Entities.Posts;

namespace ApplicationLayer.Interfaces;

public interface IPostService
{
    public Task CreatePost(Post post);
    public Task<Post> GetPostById(int id);
    public Task UpdatePost(int id);
    public Task DeletePost(int id);
}
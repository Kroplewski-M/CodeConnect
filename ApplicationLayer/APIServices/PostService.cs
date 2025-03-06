using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.APIServices;

public class PostService : IPostService
{
    public Task CreatePost(PostDTO post)
    {
        throw new NotImplementedException();
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
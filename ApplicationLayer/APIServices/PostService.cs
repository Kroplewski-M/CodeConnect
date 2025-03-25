using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Entities.Posts;
using InfrastructureLayer;

namespace ApplicationLayer.APIServices;

public class PostService(ApplicationDbContext context,AzureService azureService) : IPostService
{
    public async Task<ServiceResponse> CreatePost(PostDTO post)
    {
        var user = context.Users.FirstOrDefault(u => u.UserName == post.CreatedByUser)
                   ?? throw new NullReferenceException("User does not exist");
        var newPost  = new Post()
        {
            Content = post.Content,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
        };
        if (post?.Images?.Count > 0)
        {
            
        }
        context.Posts.Add(newPost);
        await context.SaveChangesAsync();
        return new ServiceResponse(true,"Post created successfully");
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
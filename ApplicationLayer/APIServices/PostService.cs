using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Posts;
using InfrastructureLayer;

namespace ApplicationLayer.APIServices;

public class PostService(ApplicationDbContext context,AzureService azureService) : IPostService
{
    public async Task<ServiceResponse> CreatePost(CreatePostDto createPost)
    {
        var user = context.Users.FirstOrDefault(u => u.UserName == createPost.CreatedByUserName)
                   ?? throw new NullReferenceException("User does not exist");
        var newPost  = new Post()
        {
            Content = createPost.Content,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
        };
        context.Posts.Add(newPost);
        await context.SaveChangesAsync();
        if (createPost?.Images?.Count > 0)
        {
            List<AzureImageDto> imageResults = new List<AzureImageDto>();
            foreach (var image in createPost.Images)
            {
                imageResults.Add(await azureService.UploadImage(Consts.ImageType.PostImages,image.Base64String,Guid.NewGuid().ToString(),image.Extenstion));
            }

            List<PostFile> postFiles = imageResults.Select(x => new PostFile
            {
                PostId = newPost.Id,
                FileName = x.ImageName,
            }).ToList();
            foreach (var postFile in postFiles)
            {
                newPost.Files.Add(postFile);
            }
            await context.SaveChangesAsync();
        }
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
using ApplicationLayer.DTO_s;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Posts;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.APIServices;

public class PostService(ApplicationDbContext context,IAzureService azureService, UserManager<ApplicationUser>userManager) : IPostService
{
    public async Task<ServiceResponse> CreatePost(CreatePostDto createPost)
    {
        var validator = new CreatePostDtoValidator();
        var validationResult = await validator.ValidateAsync(createPost);
        if(!validationResult.IsValid)
            return new ServiceResponse(false, "Error creating post");
        var user = await userManager.FindByNameAsync(createPost.CreatedByUserName);
        if(user == null)
            return new ServiceResponse(false, "User not found");
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

    public async Task<List<PostBasicDto>> GetUserPosts(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<PostBasicDto>();
        var user = await userManager.FindByNameAsync(username);
        if(user == null)
            return new List<PostBasicDto>();
        var posts = context.Posts
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Include(x => x.Files)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Content,
                User = x.CreatedByUser,
                CommentCount = x.Comments.Count,
                LikeCount = x.Likes.Count,
                FileNames = x.Files.Select(f => f.FileName).ToList(),
                CreatedAt = x.CreatedAt,
            })
            .ToList()
            .Select(x =>
                new PostBasicDto(
                    x.Id,
                    x.Content,
                    x.User?.UserName ?? "",
                    x.CommentCount,
                    x.LikeCount,
                    x.FileNames,
                    x.CreatedAt
                )
            )
            .ToList();
        return posts;
    }
}
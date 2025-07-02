using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Posts;
using DomainLayer.Helpers;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.APIServices;

public class PostService(ApplicationDbContext context,IAzureService azureService, UserManager<ApplicationUser>userManager) : IPostService
{
    public async Task<ServiceResponse> CreatePost(CreatePostDto createPost,string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new ServiceResponse(false, "Error creating post");
        
        var validator = new CreatePostDtoValidator();
        var validationResult = await validator.ValidateAsync(createPost);
        if(!validationResult.IsValid)
            return new ServiceResponse(false, "Error creating post");
        var user = await userManager.FindByIdAsync(userId);
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

    public async Task<PostBasicDto?> GetPostById(Guid id)
    {
        var post = await context.Posts.Where(x => x.Id == id)
            .Select(x=> new
            {
                x.Id,
                x.Content,
                FileNames = x.Files.Select(y=>y.FileName),
                CreatedBy = x.CreatedByUser,
                x.CreatedAt,
                LikeCount = x.Likes.Count(),
                CommentCount = x.Comments.Count(),
            })
            .FirstOrDefaultAsync();
        if (post != null)
            return new PostBasicDto(
                post.Id,
                post.Content,
                post.CreatedBy?.UserName ?? "",
                Helpers.GetUserImgUrl(post.CreatedBy?.ProfileImage, Consts.ImageType.ProfileImages),
                post.CommentCount,
                post.LikeCount,
                post.FileNames.Select(y => Helpers.GetAzureImgUrl(Consts.ImageType.PostImages, y)).ToList(),
                post.CreatedAt
            );
        return null;
    }

    public Task UpdatePost(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task DeletePost(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<PostBasicDto>> GetUserPosts(string username, int skip, int take)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<PostBasicDto>();
        var user = await userManager.FindByNameAsync(username);
        if(user == null)
            return new List<PostBasicDto>();
        var posts = context.Posts
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == user.Id)
            .Include(x => x.CreatedByUser)
            .Include(x => x.Files)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
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
                    Helpers.GetUserImgUrl(x.User?.ProfileImage, Consts.ImageType.ProfileImages),
                    x.CommentCount,
                    x.LikeCount,
                    x.FileNames.Select(y => Helpers.GetAzureImgUrl(Consts.ImageType.PostImages, y)).ToList(),
                    x.CreatedAt
                )
            )
            .ToList();
        return posts;
    }

    public async Task<ServiceResponse> ToggleLikePost(LikePostDto likePostDto, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new ServiceResponse(false, "Error occured while toggling like post");
        var post = context.Posts.FirstOrDefault(x => x.Id == likePostDto.PostId);
        if(post == null)
            return new ServiceResponse(false, "Post not found");
        var user = await userManager.FindByIdAsync(userId);
        if(user == null)
            return new ServiceResponse(false, "User not found");
        var existingLike = context.PostLikes.FirstOrDefault(x => x.LikedByUserId == user.Id && x.PostId == post.Id);
        if(existingLike != null)
            context.PostLikes.Remove(existingLike);
        else
        {
            var postLike = new PostLike()
            {
                PostId = post.Id,
                LikedByUserId = user.Id,
                LikedOn = DateTime.UtcNow,
            };
            post.Likes.Add(postLike);
        }
        await context.SaveChangesAsync();
        return new ServiceResponse(true, "Like added successfully");
    }

    public async Task<bool> IsUserLikingPost(Guid postId, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;
        var user = await userManager.FindByIdAsync(userId);
        if(user == null)
            return false;
        return context.PostLikes.Any(x => x.LikedByUserId == user.Id && x.PostId == postId);
    }
}
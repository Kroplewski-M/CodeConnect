using System.Xml;
using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Images;
using ApplicationLayer.DTO_s.Post;
using ApplicationLayer.ExtensionClasses;
using ApplicationLayer.Interfaces;
using DomainLayer.Constants;
using DomainLayer.Entities.Auth;
using DomainLayer.Entities.Posts;
using DomainLayer.Helpers;
using InfrastructureLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApiApplicationLayer.Interfaces;

namespace WebApiApplicationLayer.Services;

public class PostService(ApplicationDbContext context,IAzureService azureService, UserManager<ApplicationUser>userManager,IServerNotificationsService notificationsService) : IPostService
{
    public async Task<CreatePostResponseDto> CreatePost(CreatePostDto createPost,string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new CreatePostResponseDto(false, "Error creating post", null);
        
        var validator = new CreatePostDtoValidator();
        var validationResult = await validator.ValidateAsync(createPost);
        if(!validationResult.IsValid)
            return new CreatePostResponseDto(false, "Error creating post", null);
        var user = await userManager.FindByIdAsync(userId);
        if(user == null)
            return new CreatePostResponseDto(false, "User not found", null);
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
        return new CreatePostResponseDto(true,"Post created successfully", newPost.Id);
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

    public async Task<ServiceResponse> DeletePost(Guid id, string? userId = null)
    {
        if(string.IsNullOrWhiteSpace(userId))
            return new ServiceResponse(false, "user not found");
        var post = await context.Posts
            .Include(x=> x.Files)
            .FirstOrDefaultAsync(x => x.Id == id);
        if(post == null)
            return new ServiceResponse(false, "Post not found");
        if(post.CreatedByUserId != userId)
            return new ServiceResponse(false, "User did not create the post");

        var imageNames = post.Files.Select(x => x.FileName).ToList();
        try
        {
            context.CommentLikes.RemoveRange(context.CommentLikes.Where(x => x.Comment!.PostId == id));
            context.Comments.RemoveRange(context.Comments.Where(x => x.PostId == id));
            context.PostLikes.RemoveRange(context.PostLikes.Where(x => x.PostId == id));
            context.PostFiles.RemoveRange(context.PostFiles.Where(x => x.PostId == id));
            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }
        catch
        {
            return new ServiceResponse(false, "Post failed to delete");
        }
        foreach (var imageName in imageNames)
        {
            await azureService.RemoveImage(imageName, Consts.ImageType.PostImages);
        }
        return new ServiceResponse(true, "Post deleted");
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
            await notificationsService.SendNotificationAsync(post.CreatedByUserId, user.Id,Consts.NotificationTypes.PostLike, post.Id.ToString());
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

    public async Task<ServiceResponse> UpsertPostComment(Guid postId,Guid? commentId, string comment, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(comment))
            return new ServiceResponse(false, "Error occured while adding comment");
        var post = context.Posts.FirstOrDefault(x => x.Id == postId);
        if(post == null)
            return new ServiceResponse(false, "Post not found");
        var user = await userManager.FindByIdAsync(userId);
        if(user == null)
            return new ServiceResponse(false, "User not found");
        var existingComment = context.Comments.FirstOrDefault(x => x.Id == commentId && x.PostId == postId);
        var upsertComment = existingComment ?? new Comment()
        {
            Content = comment,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            PostId = post.Id,
        };
        if(commentId != null)
            upsertComment.Content = comment;
        else
        {
            context.Comments.Add(upsertComment);
        }
        await context.SaveChangesAsync();
        if(commentId == null)
            await notificationsService.SendNotificationAsync(post.CreatedByUserId, user.Id,Consts.NotificationTypes.PostComment, upsertComment.Id.ToString(), parentId: post.Id.ToString());
        
        return new ServiceResponse(true, "Comment added successfully");
    }

    public async Task<PostCommentsDto> GetCommentsForPost(Guid postId, int skip, int take, string? userId = null)
    {
        if (userId == null)
            return new PostCommentsDto(false, new List<CommentDto>());
        var post = context.Posts.FirstOrDefault(x => x.Id == postId);
        if(post == null)
            return new PostCommentsDto(false, new List<CommentDto>());
        var comments = await context.Comments.Where(x => x.PostId == postId)
            .AsNoTracking()
            .OrderByDescending(x=> x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Include(x=> x.CreatedByUser)
            .Select(x=> new {Comment= x, LikeCount = x.Likes.Count(), CurrentUserLikes = x.Likes.Any(y => y.LikedByUserId == userId)})
            .ToListAsync();
        
        var commentsDto = comments.Select(x => new CommentDto(x.Comment.Id, x.Comment.Content, x.Comment.CreatedByUser.ToUserBasicDto(),x.LikeCount, x.Comment.CreatedAt, x.CurrentUserLikes )).ToList();
        return new PostCommentsDto(true, commentsDto);
    }

    public async Task<ServiceResponse> ToggleLikeComment(Guid commentId, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
           return new ServiceResponse(false, "Error occured while adding comment"); 
        } 
        var comment = context.Comments.FirstOrDefault(x => x.Id ==  commentId);
        if (comment == null)
            return new ServiceResponse(false, "Comment not found");
        var user  = await userManager.FindByIdAsync(userId);
        if(user == null)
            return new ServiceResponse(false, "User not found");
        var existingCommentLike = context.CommentLikes.FirstOrDefault(x => x.CommentId == commentId && x.LikedByUserId == userId);
        if(existingCommentLike != null)
            context.CommentLikes.Remove(existingCommentLike);
        else
        {
            var newLike = new CommentLike()
            {
                LikedByUserId = user.Id,
                LikedOn = DateTime.UtcNow,
                CommentId = commentId,
            };
            comment.Likes.Add(newLike);
            await notificationsService.SendNotificationAsync(comment.CreatedByUserId, user.Id,Consts.NotificationTypes.CommentLike, comment.Id.ToString());
        }
        await context.SaveChangesAsync();
        return new ServiceResponse(true, "Like added successfully");
    }
}
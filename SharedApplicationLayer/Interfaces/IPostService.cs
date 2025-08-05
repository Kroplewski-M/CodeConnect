using ApplicationLayer.DTO_s;
using ApplicationLayer.DTO_s.Post;
using DomainLayer.Entities.Posts;

namespace ApplicationLayer.Interfaces;

public interface IPostService
{
    public Task<CreatePostResponseDto> CreatePost(CreatePostDto createPost,string? userId = null);
    public Task<PostBasicDto?> GetPostById(Guid id);
    public Task UpdatePost(Guid id);
    public Task DeletePost(Guid id);
    public Task<List<PostBasicDto>> GetUserPosts(string username,  int skip, int take);
    public Task<ServiceResponse> ToggleLikePost(LikePostDto likePostDto, string? userId = null);
    public Task<bool> IsUserLikingPost(Guid postId, string? userId = null);
    public Task<ServiceResponse> UpsertPostComment(Guid postId,Guid? commentId, string comment, string? userId = null);
    public Task<PostCommentsDto> GetCommentsForPost(Guid postId, int skip, int take, string? userId = null);
    
    public Task<ServiceResponse> ToggleLikeComment(Guid commentId, string? userId = null);
}
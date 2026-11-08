using ApiContracts.Posts;

namespace BlazorApp.HttpServices.Posts;

public interface IPostService
{
    Task<List<PostDto>> GetPostsAsync();
    Task<PostDto> GetPostAsync(int id, bool includeComments = false);
    Task<PostDto> AddPostAsync(CreatePostDto request);
    Task UpdatePostAsync(int id, UpdatePostDto request);
    Task DeletePostAsync(int id);
}

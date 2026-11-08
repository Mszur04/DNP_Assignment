using ApiContracts.Comments;

namespace BlazorApp.HttpServices.Comments;

public interface ICommentService
{
    Task<CommentDto> AddCommentToPostAsync(int postId, AddCommentToPostDto request);
    Task DeleteCommentAsync(int id);
}

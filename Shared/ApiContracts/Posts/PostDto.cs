using ApiContracts.Comments;

namespace ApiContracts.Posts;

/// <summary>
/// What a client gets back for a Post. AuthorUsername is included so the
/// client doesn't have to make a second request just to display who wrote it.
/// Comments is only populated when the caller asks for it
/// (GET /Posts/{id}?includeComments=true), to avoid always paying for it.
/// </summary>
public class PostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public List<CommentDto>? Comments { get; set; }
}

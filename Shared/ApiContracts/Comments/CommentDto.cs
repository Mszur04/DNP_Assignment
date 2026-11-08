namespace ApiContracts.Comments;

/// <summary>
/// What a client gets back for a Comment. AuthorUsername saves the client
/// a second lookup, same reasoning as PostDto.AuthorUsername.
/// </summary>
public class CommentDto
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int PostId { get; set; }
}

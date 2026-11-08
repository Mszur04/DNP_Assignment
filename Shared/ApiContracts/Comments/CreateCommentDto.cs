namespace ApiContracts.Comments;

/// <summary>
/// Used for the standalone POST /Comments endpoint, where the target post
/// isn't already implied by the route.
/// </summary>
public class CreateCommentDto
{
    public required string Body { get; set; }
    public required int UserId { get; set; }
    public required int PostId { get; set; }
}

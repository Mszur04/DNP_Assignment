namespace ApiContracts.Comments;

/// <summary>
/// Used for the nested POST /Posts/{postId}/Comments endpoint. No PostId
/// here - it's already in the route.
/// </summary>
public class AddCommentToPostDto
{
    public required string Body { get; set; }
    public required int UserId { get; set; }
}

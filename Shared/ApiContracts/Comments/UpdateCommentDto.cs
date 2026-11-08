namespace ApiContracts.Comments;

/// <summary>
/// What a client sends to update a Comment's text. The author and the post
/// it belongs to don't change through an update - only the body does.
/// </summary>
public class UpdateCommentDto
{
    public required string Body { get; set; }
}

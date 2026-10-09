using ApiContracts.Comments;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CommentsController(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    /// <summary>
    /// POST /Comments - the standalone create endpoint, for when the target
    /// post isn't already implied by the route (see also
    /// POST /Posts/{id}/comments on PostsController).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto request)
    {
        await EnsurePostExistsAsync(request.PostId);
        User author = await GetUserOrThrowValidationAsync(request.UserId);

        Comment comment = new()
        {
            Body = request.Body,
            UserId = request.UserId,
            PostId = request.PostId
        };
        Comment created = await commentRepository.AddAsync(comment);

        CommentDto dto = ToDto(created, author.Username);
        return Created($"/Comments/{dto.Id}", dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetSingle(int id)
    {
        Comment comment = await commentRepository.GetSingleAsync(id);
        string authorUsername = await GetAuthorUsernameAsync(comment.UserId);
        return Ok(ToDto(comment, authorUsername));
    }

    /// <summary>
    /// GET /Comments?userId=1&amp;username=substring&amp;postId=2
    /// - userId / username: comments written by a specific user, or by a
    ///   user whose username contains the substring
    /// - postId: comments on a specific post
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CommentDto>>> GetMany(
        [FromQuery] int? userId,
        [FromQuery] string? username,
        [FromQuery] int? postId)
    {
        IQueryable<Comment> query = commentRepository.GetMany();

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        if (postId.HasValue)
        {
            query = query.Where(c => c.PostId == postId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            List<int> matchingUserIds = userRepository.GetMany()
                .Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase))
                .Select(u => u.Id)
                .ToList();
            query = query.Where(c => matchingUserIds.Contains(c.UserId));
        }

        List<CommentDto> dtos = new();
        foreach (Comment comment in query.ToList())
        {
            string authorUsername = await GetAuthorUsernameAsync(comment.UserId);
            dtos.Add(ToDto(comment, authorUsername));
        }

        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(int id, [FromBody] UpdateCommentDto request)
    {
        Comment existing = await commentRepository.GetSingleAsync(id);
        existing.Body = request.Body;

        await commentRepository.UpdateAsync(existing);

        string authorUsername = await GetAuthorUsernameAsync(existing.UserId);
        return Ok(ToDto(existing, authorUsername));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        await commentRepository.DeleteAsync(id);
        return NoContent();
    }

    private async Task EnsurePostExistsAsync(int postId)
    {
        try
        {
            await postRepository.GetSingleAsync(postId);
        }
        catch (NotFoundException)
        {
            throw new ValidationException($"Post with ID '{postId}' does not exist.");
        }
    }

    private async Task<User> GetUserOrThrowValidationAsync(int userId)
    {
        try
        {
            return await userRepository.GetSingleAsync(userId);
        }
        catch (NotFoundException)
        {
            throw new ValidationException($"User with ID '{userId}' does not exist.");
        }
    }

    private async Task<string> GetAuthorUsernameAsync(int userId)
    {
        try
        {
            User user = await userRepository.GetSingleAsync(userId);
            return user.Username;
        }
        catch (NotFoundException)
        {
            return "(unknown user)";
        }
    }

    private static CommentDto ToDto(Comment comment, string authorUsername) => new()
    {
        Id = comment.Id,
        Body = comment.Body,
        UserId = comment.UserId,
        AuthorUsername = authorUsername,
        PostId = comment.PostId
    };
}

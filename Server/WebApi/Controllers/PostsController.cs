using ApiContracts.Comments;
using ApiContracts.Posts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public PostsController(IPostRepository postRepository, IUserRepository userRepository, ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto request)
    {
        User author = await GetUserOrThrowValidationAsync(request.UserId);

        Post post = new()
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId
        };
        Post created = await postRepository.AddAsync(post);

        PostDto dto = ToDto(created, author.Username);
        return Created($"/Posts/{dto.Id}", dto);
    }

    /// <summary>
    /// GET /Posts/{id}?includeComments=true - set includeComments to also
    /// get the post's comments in the same response, instead of a second
    /// round-trip to GET /Posts/{id}/comments.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSingle(int id, [FromQuery] bool includeComments = false)
    {
        Post post = await postRepository.GetSingleAsync(id);
        string authorUsername = await GetAuthorUsernameAsync(post.UserId);

        PostDto dto = ToDto(post, authorUsername);
        if (includeComments)
        {
            dto.Comments = await GetCommentDtosForPostAsync(id);
        }

        return Ok(dto);
    }

    /// <summary>
    /// GET /Posts?title=substring&amp;userId=1&amp;username=substring
    /// - title: posts whose title contains the substring
    /// - userId: posts written by that specific user
    /// - username: posts written by a user whose username contains the substring
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PostDto>>> GetMany(
        [FromQuery] string? title,
        [FromQuery] int? userId,
        [FromQuery] string? username)
    {
        IQueryable<Post> query = postRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(p => p.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            List<int> matchingUserIds = userRepository.GetMany()
                .Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase))
                .Select(u => u.Id)
                .ToList();
            query = query.Where(p => matchingUserIds.Contains(p.UserId));
        }

        List<PostDto> dtos = new();
        foreach (Post post in query.ToList())
        {
            string authorUsername = await GetAuthorUsernameAsync(post.UserId);
            dtos.Add(ToDto(post, authorUsername));
        }

        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDto>> UpdatePost(int id, [FromBody] UpdatePostDto request)
    {
        Post existing = await postRepository.GetSingleAsync(id);
        User author = await GetUserOrThrowValidationAsync(request.UserId);

        existing.Title = request.Title;
        existing.Body = request.Body;
        existing.UserId = request.UserId;

        await postRepository.UpdateAsync(existing);
        return Ok(ToDto(existing, author.Username));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        await postRepository.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>GET /Posts/{id}/comments - all comments on this post.</summary>
    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<List<CommentDto>>> GetCommentsForPost(int id)
    {
        await postRepository.GetSingleAsync(id);
        List<CommentDto> dtos = await GetCommentDtosForPostAsync(id);
        return Ok(dtos);
    }

    /// <summary>POST /Posts/{id}/comments - add a comment to this specific post.</summary>
    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddCommentToPost(int id, [FromBody] AddCommentToPostDto request)
    {
        await postRepository.GetSingleAsync(id);
        User author = await GetUserOrThrowValidationAsync(request.UserId);

        Comment comment = new()
        {
            Body = request.Body,
            UserId = request.UserId,
            PostId = id
        };
        Comment created = await commentRepository.AddAsync(comment);

        CommentDto dto = new()
        {
            Id = created.Id,
            Body = created.Body,
            UserId = created.UserId,
            AuthorUsername = author.Username,
            PostId = created.PostId
        };
        return Created($"/Posts/{id}/comments/{dto.Id}", dto);
    }

    private async Task<List<CommentDto>> GetCommentDtosForPostAsync(int postId)
    {
        List<Comment> comments = commentRepository.GetMany().Where(c => c.PostId == postId).ToList();

        List<CommentDto> dtos = new();
        foreach (Comment comment in comments)
        {
            string authorUsername = await GetAuthorUsernameAsync(comment.UserId);
            dtos.Add(new CommentDto
            {
                Id = comment.Id,
                Body = comment.Body,
                UserId = comment.UserId,
                AuthorUsername = authorUsername,
                PostId = comment.PostId
            });
        }

        return dtos;
    }

    /// <summary>
    /// Verifies the referenced user exists, for business logic checks when
    /// creating/updating a post. A missing user here is the client's fault
    /// (a bad Id in the request body), so it's surfaced as a 400, not a 404.
    /// </summary>
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

    private static PostDto ToDto(Post post, string authorUsername) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Body = post.Body,
        UserId = post.UserId,
        AuthorUsername = authorUsername
    };
}

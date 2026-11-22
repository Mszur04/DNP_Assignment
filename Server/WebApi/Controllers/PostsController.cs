using ApiContracts.Comments;
using ApiContracts.Posts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    ///
    /// Part 7: this is now a single query - the author's and each
    /// comment's author's username are pulled in via the Post/Comment
    /// navigation properties (Post.User, Comment.User) directly inside the
    /// projection, which EF Core translates into the joins needed to fetch
    /// everything in one round-trip to the database, instead of the
    /// separate per-entity lookups the file-based version needed.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSingle(int id, [FromQuery] bool includeComments = false)
    {
        PostDto? dto = await postRepository.GetMany()
            .Where(p => p.Id == id)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                AuthorUsername = p.User!.Username,
                Comments = includeComments
                    ? p.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Body = c.Body,
                        UserId = c.UserId,
                        AuthorUsername = c.User!.Username,
                        PostId = c.PostId
                    }).ToList()
                    : null
            })
            .FirstOrDefaultAsync();

        if (dto is null)
        {
            throw new NotFoundException($"Post with id {id} not found");
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
            string titleLower = title.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(titleLower));
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            string usernameLower = username.ToLower();
            List<int> matchingUserIds = await userRepository.GetMany()
                .Where(u => u.Username.ToLower().Contains(usernameLower))
                .Select(u => u.Id)
                .ToListAsync();
            query = query.Where(p => matchingUserIds.Contains(p.UserId));
        }

        List<PostDto> dtos = await query
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                AuthorUsername = p.User!.Username
            })
            .ToListAsync();

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
        return await commentRepository.GetMany()
            .Where(c => c.PostId == postId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                UserId = c.UserId,
                AuthorUsername = c.User!.Username,
                PostId = c.PostId
            })
            .ToListAsync();
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

    private static PostDto ToDto(Post post, string authorUsername) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Body = post.Body,
        UserId = post.UserId,
        AuthorUsername = authorUsername
    };
}

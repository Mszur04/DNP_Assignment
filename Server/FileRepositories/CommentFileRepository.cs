using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class CommentFileRepository : ICommentRepository
{
    private const string FilePath = "comments.json";

    public CommentFileRepository()
    {
        List<Comment> seedComments = new()
        {
            new Comment { Id = 1, Body = "Hello there!", UserId = 2, PostId = 1 },
            new Comment { Id = 2, Body = "Welcome!", UserId = 3, PostId = 1 },
            new Comment { Id = 3, Body = "Pineapple is a crime.", UserId = 1, PostId = 2 },
            new Comment { Id = 4, Body = "Keep at it, it clicks eventually.", UserId = 2, PostId = 3 },
            new Comment { Id = 5, Body = "Try the trail near the lake, it's beautiful.", UserId = 3, PostId = 4 }
        };
        JsonFileHelper.EnsureFileExists(FilePath, seedComments);
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(FilePath);

        comment.Id = comments.Count > 0
            ? comments.Max(c => c.Id) + 1
            : 1;
        comments.Add(comment);

        await JsonFileHelper.SaveAsync(FilePath, comments);
        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(FilePath);

        Comment? existingComment = comments.SingleOrDefault(c => c.Id == comment.Id);
        if (existingComment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{comment.Id}' not found");
        }

        comments.Remove(existingComment);
        comments.Add(comment);

        await JsonFileHelper.SaveAsync(FilePath, comments);
    }

    public async Task DeleteAsync(int id)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(FilePath);

        Comment? commentToRemove = comments.SingleOrDefault(c => c.Id == id);
        if (commentToRemove is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);
        await JsonFileHelper.SaveAsync(FilePath, comments);
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        List<Comment> comments = await JsonFileHelper.LoadAsync<Comment>(FilePath);

        Comment? comment = comments.SingleOrDefault(c => c.Id == id);
        if (comment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        return comment;
    }

    public IQueryable<Comment> GetMany()
    {
        List<Comment> comments = JsonFileHelper.Load<Comment>(FilePath);
        return comments.AsQueryable();
    }
}

using Cli.Helpers;
using Entities;
using RepositoryContracts;

namespace Cli.Views;

/// <summary>
/// Handles all "manage comments" CLI interactions: add a comment to a post, update,
/// delete, and list comments made by a specific user.
/// </summary>
public class CommentView
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CommentView(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowMenuAsync()
    {
        bool exit = false;
        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("=== Manage Comments ===");
            Console.WriteLine("1. Add comment to a post");
            Console.WriteLine("2. Update existing comment");
            Console.WriteLine("3. Delete comment");
            Console.WriteLine("4. View comments by user");
            Console.WriteLine("5. Back to main menu");

            string choice = ConsoleInput.ReadNonEmptyString("Choose an option");
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await AddCommentAsync();
                    break;
                case "2":
                    await UpdateCommentAsync();
                    break;
                case "3":
                    await DeleteCommentAsync();
                    break;
                case "4":
                    await ListCommentsByUserAsync();
                    break;
                case "5":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }

            if (!exit)
            {
                ConsoleInput.Pause();
            }
        }
    }

    private async Task AddCommentAsync()
    {
        Console.WriteLine("--- Add comment to a post ---");
        int postId = ConsoleInput.ReadInt("Post Id");

        try
        {
            await postRepository.GetSingleAsync(postId);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No post found with Id {postId}. Comment was not added.");
            return;
        }

        int userId = ConsoleInput.ReadInt("Your user Id");
        try
        {
            await userRepository.GetSingleAsync(userId);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No user found with Id {userId}. Comment was not added.");
            return;
        }

        string body = ConsoleInput.ReadNonEmptyString("Comment");

        Comment comment = new()
        {
            Body = body,
            UserId = userId,
            PostId = postId
        };

        Comment created = await commentRepository.AddAsync(comment);
        Console.WriteLine($"Comment created with Id {created.Id}.");
    }

    private async Task UpdateCommentAsync()
    {
        Console.WriteLine("--- Update existing comment ---");
        int id = ConsoleInput.ReadInt("Comment Id to update");

        Comment existing;
        try
        {
            existing = await commentRepository.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No comment found with Id {id}.");
            return;
        }

        string newBody = ConsoleInput.ReadNonEmptyString($"New body (was: \"{existing.Body}\")");
        existing.Body = newBody;

        await commentRepository.UpdateAsync(existing);
        Console.WriteLine("Comment updated.");
    }

    private async Task DeleteCommentAsync()
    {
        Console.WriteLine("--- Delete comment ---");
        int id = ConsoleInput.ReadInt("Comment Id to delete");

        try
        {
            await commentRepository.DeleteAsync(id);
            Console.WriteLine("Comment deleted.");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No comment found with Id {id}.");
        }
    }

    private async Task ListCommentsByUserAsync()
    {
        Console.WriteLine("--- View comments by user ---");
        int userId = ConsoleInput.ReadInt("User Id");

        try
        {
            await userRepository.GetSingleAsync(userId);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No user found with Id {userId}.");
            return;
        }

        List<Comment> comments = commentRepository.GetMany()
            .Where(c => c.UserId == userId)
            .ToList();

        if (comments.Count == 0)
        {
            Console.WriteLine("This user has not made any comments.");
            return;
        }

        foreach (Comment comment in comments)
        {
            string postTitle = await GetPostTitleOrUnknownAsync(comment.PostId);
            Console.WriteLine($"[{comment.Id}] on \"{postTitle}\": {comment.Body}");
        }
    }

    private async Task<string> GetPostTitleOrUnknownAsync(int postId)
    {
        try
        {
            Post post = await postRepository.GetSingleAsync(postId);
            return post.Title;
        }
        catch (InvalidOperationException)
        {
            return "(unknown post)";
        }
    }
}

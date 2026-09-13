using Cli.Helpers;
using Entities;
using RepositoryContracts;

namespace Cli.Views;

/// <summary>
/// Handles all "manage posts" CLI interactions: create, update, delete, list overview,
/// view a single post (with its comments), and view posts by a specific user.
/// </summary>
public class PostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public PostView(IPostRepository postRepository, IUserRepository userRepository, ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
    }

    public async Task ShowMenuAsync()
    {
        bool exit = false;
        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("=== Manage Posts ===");
            Console.WriteLine("1. Create new post");
            Console.WriteLine("2. Update existing post");
            Console.WriteLine("3. Delete post");
            Console.WriteLine("4. View posts overview");
            Console.WriteLine("5. View single post");
            Console.WriteLine("6. View posts by user");
            Console.WriteLine("7. Back to main menu");

            string choice = ConsoleInput.ReadNonEmptyString("Choose an option");
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await CreatePostAsync();
                    break;
                case "2":
                    await UpdatePostAsync();
                    break;
                case "3":
                    await DeletePostAsync();
                    break;
                case "4":
                    ShowOverview(postRepository.GetMany());
                    break;
                case "5":
                    await ViewSinglePostAsync();
                    break;
                case "6":
                    await ViewPostsByUserAsync();
                    break;
                case "7":
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

    private async Task CreatePostAsync()
    {
        Console.WriteLine("--- Create new post ---");
        string title = ConsoleInput.ReadNonEmptyString("Title");
        string body = ConsoleInput.ReadNonEmptyString("Body");
        int userId = ConsoleInput.ReadInt("Author user Id");

        if (!await UserExistsAsync(userId))
        {
            Console.WriteLine($"No user found with Id {userId}. Post was not created.");
            return;
        }

        Post post = new()
        {
            Title = title,
            Body = body,
            UserId = userId
        };

        Post created = await postRepository.AddAsync(post);
        Console.WriteLine($"Post created with Id {created.Id}.");
    }

    private async Task UpdatePostAsync()
    {
        Console.WriteLine("--- Update existing post ---");
        int id = ConsoleInput.ReadInt("Post Id to update");

        Post existing;
        try
        {
            existing = await postRepository.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No post found with Id {id}.");
            return;
        }

        Console.WriteLine("Leave a field blank to keep its current value.");

        string? newTitle = ConsoleInput.ReadOptionalString($"Title ({existing.Title})");
        string? newBody = ConsoleInput.ReadOptionalString("Body (unchanged)");
        int? newUserId = ConsoleInput.ReadOptionalInt($"Author user Id ({existing.UserId})");

        if (newUserId.HasValue && !await UserExistsAsync(newUserId.Value))
        {
            Console.WriteLine($"No user found with Id {newUserId.Value}. Post was not updated.");
            return;
        }

        existing.Title = newTitle ?? existing.Title;
        existing.Body = newBody ?? existing.Body;
        existing.UserId = newUserId ?? existing.UserId;

        await postRepository.UpdateAsync(existing);
        Console.WriteLine("Post updated.");
    }

    private async Task DeletePostAsync()
    {
        Console.WriteLine("--- Delete post ---");
        int id = ConsoleInput.ReadInt("Post Id to delete");

        try
        {
            await postRepository.DeleteAsync(id);
            Console.WriteLine("Post deleted.");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No post found with Id {id}.");
        }
    }

    private async Task ViewSinglePostAsync()
    {
        Console.WriteLine("--- View single post ---");
        int id = ConsoleInput.ReadInt("Post Id");

        Post post;
        try
        {
            post = await postRepository.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No post found with Id {id}.");
            return;
        }

        string authorName = await GetUsernameOrUnknownAsync(post.UserId);

        Console.WriteLine();
        Console.WriteLine($"[{post.Id}] {post.Title}");
        Console.WriteLine($"By: {authorName}");
        Console.WriteLine();
        Console.WriteLine(post.Body);
        Console.WriteLine();

        List<Comment> comments = commentRepository.GetMany()
            .Where(c => c.PostId == post.Id)
            .ToList();

        Console.WriteLine($"--- Comments ({comments.Count}) ---");
        if (comments.Count == 0)
        {
            Console.WriteLine("No comments yet.");
            return;
        }

        foreach (Comment comment in comments)
        {
            string commenterName = await GetUsernameOrUnknownAsync(comment.UserId);
            Console.WriteLine($"[{comment.Id}] {commenterName}: {comment.Body}");
        }
    }

    private async Task ViewPostsByUserAsync()
    {
        Console.WriteLine("--- View posts by user ---");
        int userId = ConsoleInput.ReadInt("User Id");

        if (!await UserExistsAsync(userId))
        {
            Console.WriteLine($"No user found with Id {userId}.");
            return;
        }

        IQueryable<Post> posts = postRepository.GetMany().Where(p => p.UserId == userId);
        ShowOverview(posts);
    }

    private static void ShowOverview(IEnumerable<Post> posts)
    {
        List<Post> list = posts.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("No posts found.");
            return;
        }

        Console.WriteLine("--- Posts ---");
        foreach (Post post in list)
        {
            Console.WriteLine($"[{post.Id}] {post.Title}");
        }
    }

    private async Task<bool> UserExistsAsync(int userId)
    {
        try
        {
            await userRepository.GetSingleAsync(userId);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private async Task<string> GetUsernameOrUnknownAsync(int userId)
    {
        try
        {
            User user = await userRepository.GetSingleAsync(userId);
            return user.Username;
        }
        catch (InvalidOperationException)
        {
            return "(unknown user)";
        }
    }
}

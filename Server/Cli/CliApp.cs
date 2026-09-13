using Cli.Helpers;
using Cli.Views;
using RepositoryContracts;

namespace Cli;

/// <summary>
/// Entry point for the CLI's UI logic. Owns the top-level menu and delegates
/// to the individual entity views. Repositories are injected so the same
/// instances (and therefore the same data) are shared across every view.
/// </summary>
public class CliApp
{
    private readonly UserView userView;
    private readonly PostView postView;
    private readonly CommentView commentView;

    public CliApp(IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository)
    {
        userView = new UserView(userRepository);
        postView = new PostView(postRepository, userRepository, commentRepository);
        commentView = new CommentView(commentRepository, postRepository, userRepository);
    }

    public async Task StartAsync()
    {
        bool exit = false;
        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("=== Forum CLI ===");
            Console.WriteLine("1. Manage users");
            Console.WriteLine("2. Manage posts");
            Console.WriteLine("3. Manage comments");
            Console.WriteLine("4. Exit");

            string choice = ConsoleInput.ReadNonEmptyString("Choose an option");
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await userView.ShowMenuAsync();
                    break;
                case "2":
                    await postView.ShowMenuAsync();
                    break;
                case "3":
                    await commentView.ShowMenuAsync();
                    break;
                case "4":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Unknown option.");
                    ConsoleInput.Pause();
                    break;
            }
        }

        Console.WriteLine("Goodbye!");
    }
}

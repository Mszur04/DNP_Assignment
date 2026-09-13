using Cli.Helpers;
using Entities;
using RepositoryContracts;

namespace Cli.Views;

/// <summary>
/// Handles all "manage users" CLI interactions: create, update, delete, list, search.
/// The repository is injected through the constructor so every view shares the same
/// repository instances that were created once in Program.cs.
/// </summary>
public class UserView
{
    private readonly IUserRepository userRepository;

    public UserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowMenuAsync()
    {
        bool exit = false;
        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("=== Manage Users ===");
            Console.WriteLine("1. Create new user");
            Console.WriteLine("2. Update existing user");
            Console.WriteLine("3. Delete user");
            Console.WriteLine("4. List all users");
            Console.WriteLine("5. Search users by username");
            Console.WriteLine("6. Back to main menu");

            string choice = ConsoleInput.ReadNonEmptyString("Choose an option");
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await CreateUserAsync();
                    break;
                case "2":
                    await UpdateUserAsync();
                    break;
                case "3":
                    await DeleteUserAsync();
                    break;
                case "4":
                    ListUsers(userRepository.GetMany());
                    break;
                case "5":
                    await SearchUsersAsync();
                    break;
                case "6":
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

    private async Task CreateUserAsync()
    {
        Console.WriteLine("--- Create new user ---");
        string username = ConsoleInput.ReadNonEmptyString("Username");

        if (UsernameIsTaken(username))
        {
            Console.WriteLine($"Username '{username}' is already taken. User was not created.");
            return;
        }

        string password = ConsoleInput.ReadNonEmptyString("Password");
        string email = ConsoleInput.ReadNonEmptyString("Email");

        User user = new()
        {
            Username = username,
            Password = password,
            Email = email
        };

        User created = await userRepository.AddAsync(user);
        Console.WriteLine($"User created with Id {created.Id}.");
    }

    private async Task UpdateUserAsync()
    {
        Console.WriteLine("--- Update existing user ---");
        int id = ConsoleInput.ReadInt("User Id to update");

        User existing;
        try
        {
            existing = await userRepository.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No user found with Id {id}.");
            return;
        }

        Console.WriteLine("Leave a field blank to keep its current value.");

        string? newUsername = ConsoleInput.ReadOptionalString($"Username ({existing.Username})");
        if (newUsername is not null && !string.Equals(newUsername, existing.Username, StringComparison.OrdinalIgnoreCase)
            && UsernameIsTaken(newUsername))
        {
            Console.WriteLine($"Username '{newUsername}' is already taken. User was not updated.");
            return;
        }

        string? newPassword = ConsoleInput.ReadOptionalString("Password (unchanged)");
        string? newEmail = ConsoleInput.ReadOptionalString($"Email ({existing.Email})");

        existing.Username = newUsername ?? existing.Username;
        existing.Password = newPassword ?? existing.Password;
        existing.Email = newEmail ?? existing.Email;

        await userRepository.UpdateAsync(existing);
        Console.WriteLine("User updated.");
    }

    private async Task DeleteUserAsync()
    {
        Console.WriteLine("--- Delete user ---");
        int id = ConsoleInput.ReadInt("User Id to delete");

        try
        {
            await userRepository.DeleteAsync(id);
            Console.WriteLine("User deleted.");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No user found with Id {id}.");
        }
    }

    private async Task SearchUsersAsync()
    {
        Console.WriteLine("--- Search users by username ---");
        string term = ConsoleInput.ReadNonEmptyString("Search term");

        IQueryable<User> matches = userRepository.GetMany()
            .Where(u => u.Username.Contains(term, StringComparison.OrdinalIgnoreCase));

        ListUsers(matches);
        await Task.CompletedTask;
    }

    private static void ListUsers(IEnumerable<User> users)
    {
        List<User> list = users.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("No users found.");
            return;
        }

        Console.WriteLine("--- Users ---");
        foreach (User user in list)
        {
            Console.WriteLine($"[{user.Id}] {user.Username} <{user.Email}>");
        }
    }

    private bool UsernameIsTaken(string username)
    {
        return userRepository.GetMany()
            .Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }
}

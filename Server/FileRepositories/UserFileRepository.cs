using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : IUserRepository
{
    private const string FilePath = "users.json";

    public UserFileRepository()
    {
        List<User> seedUsers = new()
        {
            new User { Id = 1, Username = "alice", Password = "password1", Email = "alice@example.com" },
            new User { Id = 2, Username = "bob", Password = "password2", Email = "bob@example.com" },
            new User { Id = 3, Username = "carol", Password = "password3", Email = "carol@example.com" }
        };
        JsonFileHelper.EnsureFileExists(FilePath, seedUsers);
    }

    public async Task<User> AddAsync(User user)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(FilePath);

        user.Id = users.Count > 0
            ? users.Max(u => u.Id) + 1
            : 1;
        users.Add(user);

        await JsonFileHelper.SaveAsync(FilePath, users);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(FilePath);

        User? existingUser = users.SingleOrDefault(u => u.Id == user.Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);
        users.Add(user);

        await JsonFileHelper.SaveAsync(FilePath, users);
    }

    public async Task DeleteAsync(int id)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(FilePath);

        User? userToRemove = users.SingleOrDefault(u => u.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);
        await JsonFileHelper.SaveAsync(FilePath, users);
    }

    public async Task<User> GetSingleAsync(int id)
    {
        List<User> users = await JsonFileHelper.LoadAsync<User>(FilePath);

        User? user = users.SingleOrDefault(u => u.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        return user;
    }

    public IQueryable<User> GetMany()
    {
        List<User> users = JsonFileHelper.Load<User>(FilePath);
        return users.AsQueryable();
    }
}

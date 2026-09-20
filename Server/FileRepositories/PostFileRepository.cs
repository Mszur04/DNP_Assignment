using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class PostFileRepository : IPostRepository
{
    private const string FilePath = "posts.json";

    public PostFileRepository()
    {
        List<Post> seedPosts = new()
        {
            new Post { Id = 1, Title = "Welcome to the forum", Body = "This is the very first post. Say hello!", UserId = 1 },
            new Post { Id = 2, Title = "Best pizza toppings?", Body = "I'm team pineapple, fight me.", UserId = 2 },
            new Post { Id = 3, Title = "Learning C#", Body = "Async/await is starting to make sense.", UserId = 3 },
            new Post { Id = 4, Title = "Weekend plans", Body = "Anyone got recommendations for hiking trails?", UserId = 1 }
        };
        JsonFileHelper.EnsureFileExists(FilePath, seedPosts);
    }

    public async Task<Post> AddAsync(Post post)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(FilePath);

        post.Id = posts.Count > 0
            ? posts.Max(p => p.Id) + 1
            : 1;
        posts.Add(post);

        await JsonFileHelper.SaveAsync(FilePath, posts);
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(FilePath);

        Post? existingPost = posts.SingleOrDefault(p => p.Id == post.Id);
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);

        await JsonFileHelper.SaveAsync(FilePath, posts);
    }

    public async Task DeleteAsync(int id)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(FilePath);

        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);
        await JsonFileHelper.SaveAsync(FilePath, posts);
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        List<Post> posts = await JsonFileHelper.LoadAsync<Post>(FilePath);

        Post? post = posts.SingleOrDefault(p => p.Id == id);
        if (post is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        return post;
    }

    public IQueryable<Post> GetMany()
    {
        List<Post> posts = JsonFileHelper.Load<Post>(FilePath);
        return posts.AsQueryable();
    }
}

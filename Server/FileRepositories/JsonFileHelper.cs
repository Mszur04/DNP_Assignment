using System.Text.Json;

namespace FileRepositories;

/// <summary>
/// Shared load/save logic for the JSON-backed file repositories, so each
/// repository doesn't have to repeat its own serialization boilerplate.
/// </summary>
internal static class JsonFileHelper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Makes sure a data file exists, creating it with the given seed data if it doesn't.
    /// This only runs once - after that, whatever is in the file is what persists.
    /// </summary>
    public static void EnsureFileExists<T>(string filePath, List<T> seedData)
    {
        if (!File.Exists(filePath))
        {
            string json = JsonSerializer.Serialize(seedData, SerializerOptions);
            File.WriteAllText(filePath, json);
        }
    }

    public static async Task<List<T>> LoadAsync<T>(string filePath)
    {
        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    /// <summary>
    /// Synchronous counterpart to LoadAsync, used by GetMany(), whose signature
    /// (from the repository interface) is not asynchronous.
    /// </summary>
    public static List<T> Load<T>(string filePath)
    {
        string json = File.ReadAllTextAsync(filePath).Result;
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public static async Task SaveAsync<T>(string filePath, List<T> items)
    {
        string json = JsonSerializer.Serialize(items, SerializerOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}

using System.Text.Json.Serialization;

namespace Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Navigation properties (Part 7 - EFC associations). These are only
    // ever populated by EF Core (via Include/projections in the Web API);
    // the file-based repositories (Part 3) never touch them, which is why
    // they're marked [JsonIgnore] - without that, serializing a User would
    // recurse into its Posts/Comments, which each reference their User
    // right back, causing a JSON cycle exception.
    [JsonIgnore]
    public ICollection<Post> Posts { get; set; } = new List<Post>();

    [JsonIgnore]
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

using System.Text.Json.Serialization;

namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int PostId { get; set; }

    // Navigation properties (Part 7 - EFC associations). See the note on
    // User's navigation properties for why these are [JsonIgnore]'d.
    [JsonIgnore]
    public User? User { get; set; }

    [JsonIgnore]
    public Post? Post { get; set; }
}

namespace ApiContracts.Users;

/// <summary>
/// What a client gets back for a User. Deliberately excludes Password -
/// we never want to expose that over the API, including from the login
/// endpoint.
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

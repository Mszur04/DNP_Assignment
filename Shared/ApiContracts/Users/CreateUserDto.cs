namespace ApiContracts.Users;

/// <summary>
/// What a client sends to create a new User. No Id - the server assigns that.
/// </summary>
public class CreateUserDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
}

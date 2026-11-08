namespace ApiContracts.Users;

/// <summary>
/// What a client sends to replace an existing User's data (PUT /Users/{id}).
/// The Id comes from the route, not the body.
/// </summary>
public class UpdateUserDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
}

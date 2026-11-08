namespace ApiContracts.Auth;

/// <summary>
/// What a client sends to POST /auth/login.
/// </summary>
public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

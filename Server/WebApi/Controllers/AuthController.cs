using ApiContracts.Auth;
using ApiContracts.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

/// <summary>
/// Handles login. Deliberately simple/not very secure, as the assignment
/// intends - just enough to tell different users apart.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository userRepository;

    public AuthController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpPost("login")]
    public ActionResult<UserDto> Login([FromBody] LoginRequest request)
    {
        User? user = userRepository.GetMany()
            .SingleOrDefault(u => string.Equals(u.Username, request.Username, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (user.Password != request.Password)
        {
            return Unauthorized("Invalid username or password.");
        }

        UserDto dto = new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
        return Ok(dto);
    }
}

using ApiContracts.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepository;

    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto request)
    {
        EnsureUsernameIsAvailable(request.Username);

        User user = new()
        {
            Username = request.Username,
            Password = request.Password,
            Email = request.Email
        };
        User created = await userRepository.AddAsync(user);

        UserDto dto = ToDto(created);
        return Created($"/Users/{dto.Id}", dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetSingle(int id)
    {
        User user = await userRepository.GetSingleAsync(id);
        return Ok(ToDto(user));
    }

    /// <summary>
    /// GET /Users?username=substring - lists users, optionally filtered to
    /// those whose username contains the given substring.
    /// </summary>
    [HttpGet]
    public ActionResult<List<UserDto>> GetMany([FromQuery] string? username)
    {
        IQueryable<User> query = userRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(username))
        {
            query = query.Where(u => u.Username.Contains(username, StringComparison.OrdinalIgnoreCase));
        }

        List<UserDto> dtos = query.Select(ToDto).ToList();
        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto request)
    {
        User existing = await userRepository.GetSingleAsync(id);

        if (!string.Equals(existing.Username, request.Username, StringComparison.OrdinalIgnoreCase))
        {
            EnsureUsernameIsAvailable(request.Username, excludingUserId: id);
        }

        existing.Username = request.Username;
        existing.Password = request.Password;
        existing.Email = request.Email;

        await userRepository.UpdateAsync(existing);
        return Ok(ToDto(existing));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        await userRepository.DeleteAsync(id);
        return NoContent();
    }

    private void EnsureUsernameIsAvailable(string username, int? excludingUserId = null)
    {
        bool taken = userRepository.GetMany()
            .Any(u => u.Id != excludingUserId && string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        if (taken)
        {
            throw new ValidationException($"Username '{username}' is already taken.");
        }
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email
    };
}

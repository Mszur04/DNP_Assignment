using ApiContracts.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        await EnsureUsernameIsAvailableAsync(request.Username);

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
    public async Task<ActionResult<List<UserDto>>> GetMany([FromQuery] string? username)
    {
        IQueryable<User> query = userRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(username))
        {
            // Database providers (EF Core/SQLite included) can't translate
            // the string.Contains(string, StringComparison) overload, so
            // case-insensitive matching is done via ToLower() instead -
            // this one does translate to SQL.
            string usernameLower = username.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(usernameLower));
        }

        List<UserDto> dtos = await query
            .Select(u => new UserDto { Id = u.Id, Username = u.Username, Email = u.Email })
            .ToListAsync();

        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto request)
    {
        User existing = await userRepository.GetSingleAsync(id);

        if (!string.Equals(existing.Username, request.Username, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureUsernameIsAvailableAsync(request.Username, excludingUserId: id);
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

    private async Task EnsureUsernameIsAvailableAsync(string username, int? excludingUserId = null)
    {
        string usernameLower = username.ToLower();
        bool taken = await userRepository.GetMany()
            .AnyAsync(u => u.Id != excludingUserId && u.Username.ToLower() == usernameLower);

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

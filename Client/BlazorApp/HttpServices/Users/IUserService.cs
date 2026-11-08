using ApiContracts.Users;

namespace BlazorApp.HttpServices.Users;

public interface IUserService
{
    Task<UserDto> AddUserAsync(CreateUserDto request);
}

using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using ApiContracts.Auth;
using ApiContracts.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorApp.Auth;

/// <summary>
/// Provides authentication state to the Blazor framework. Logging in calls
/// the Web API's auth/login endpoint, turns the returned UserDto into a
/// ClaimsPrincipal, and caches it in the browser's session storage so a
/// page refresh (or opening another tab) doesn't silently log the user out.
/// </summary>
public class SimpleAuthProvider : AuthenticationStateProvider
{
    private const string SessionStorageKey = "currentUser";

    private readonly HttpClient httpClient;
    private readonly IJSRuntime jsRuntime;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SimpleAuthProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        this.httpClient = httpClient;
        this.jsRuntime = jsRuntime;
    }

    public async Task Login(string username, string password)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest { Username = username, Password = password });

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(content);
        }

        UserDto userDto = JsonSerializer.Deserialize<UserDto>(content, JsonOptions)!;

        string serialisedData = JsonSerializer.Serialize(userDto);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", SessionStorageKey, serialisedData);

        ClaimsPrincipal claimsPrincipal = BuildClaimsPrincipal(userDto);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task Logout()
    {
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionStorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? userAsJson;
        try
        {
            userAsJson = await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", SessionStorageKey);
        }
        catch (InvalidOperationException)
        {
            // JS interop isn't available yet during prerendering - treat as logged out for now.
            return new AuthenticationState(new ClaimsPrincipal());
        }

        if (string.IsNullOrEmpty(userAsJson))
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }

        UserDto userDto = JsonSerializer.Deserialize<UserDto>(userAsJson, JsonOptions)!;
        return new AuthenticationState(BuildClaimsPrincipal(userDto));
    }

    private static ClaimsPrincipal BuildClaimsPrincipal(UserDto userDto)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, userDto.Username),
            new Claim("Id", userDto.Id.ToString()),
            new Claim(ClaimTypes.Email, userDto.Email)
        };
        ClaimsIdentity identity = new(claims, "apiauth");
        return new ClaimsPrincipal(identity);
    }
}

using System.Net.Http.Json;
using RaceDay.Api.Dtos.Auth;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

/// <summary>
/// The session cookie isn't captured automatically by HttpClient in test
/// scenarios, so this helper registers + logs in a fresh user and returns
/// the raw Set-Cookie value to attach to subsequent requests.
/// </summary>
public static class TestAuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string role, string? emailOverride = null)
    {
        var email = emailOverride ?? $"{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = "P@ssword123!",
            Role = role
        });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "P@ssword123!"
        });
        loginResponse.EnsureSuccessStatusCode();

        var setCookie = loginResponse.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.First()
            : throw new InvalidOperationException("Login did not set a session cookie.");

        // Set-Cookie looks like "RaceDay.Session=xyz; path=/; ..." — we only need the name=value pair.
        return setCookie.Split(';')[0];
    }

    public static HttpRequestMessage WithSession(this HttpRequestMessage request, string cookie)
    {
        request.Headers.Add("Cookie", cookie);
        return request;
    }
}

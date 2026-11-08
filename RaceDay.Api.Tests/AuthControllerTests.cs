using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Auth;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Alice",
            LastName = "Runner",
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "P@ssword123!",
            Role = RoleNames.Participant
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest
        {
            FirstName = "Bob",
            LastName = "Racer",
            Email = email,
            Password = "P@ssword123!",
            Role = RoleNames.Participant
        };

        var first = await _client.PostAsJsonAsync("/api/auth/register", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/api/auth/register", request);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithUnknownRole_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Eve",
            LastName = "Unknown",
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "P@ssword123!",
            Role = "SuperAdmin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOkAndSessionCookie()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Cara",
            LastName = "Sprinter",
            Email = email,
            Password = "P@ssword123!",
            Role = RoleNames.Organiser
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "P@ssword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out _).Should().BeTrue("a successful login must start a server-side session");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Dan",
            LastName = "Marathoner",
            Email = email,
            Password = "P@ssword123!",
            Role = RoleNames.Participant
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "nobody@example.com",
            Password = "Whatever123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

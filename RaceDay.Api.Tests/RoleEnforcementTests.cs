using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

/// <summary>
/// Covers the brief's explicit requirement to test "authentication/
/// authorization failures such as unauthenticated access and forbidden
/// role access", using Events (Organiser-only) as the protected resource.
/// </summary>
public class RoleEnforcementTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoleEnforcementTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static EventRequest SampleEvent() => new()
    {
        EventName = "Test 10K",
        EventDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
        StartTime = new TimeOnly(7, 0),
        Location = "Test City",
        EventType = EventType.Run,
        MaximumParticipants = 100,
        RegistrationDeadline = DateOnly.FromDateTime(DateTime.Today.AddDays(20))
    };

    [Fact]
    public async Task CreateEvent_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/events", SampleEvent());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbidden()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(SampleEvent())
        }.WithSession(cookie);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(SampleEvent())
        }.WithSession(cookie);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetEvents_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/events");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEvents_AsEitherRole_ReturnsOk()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/events").WithSession(cookie);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

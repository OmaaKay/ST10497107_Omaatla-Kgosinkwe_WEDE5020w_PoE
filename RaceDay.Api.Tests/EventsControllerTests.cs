using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class EventsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static EventRequest SampleEvent(string name = "Sunrise 5K") => new()
    {
        EventName = name,
        EventDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
        StartTime = new TimeOnly(7, 0),
        Location = "Johannesburg",
        DistanceKm = 5,
        EventType = EventType.Run,
        MaximumParticipants = 200,
        RegistrationDeadline = DateOnly.FromDateTime(DateTime.Today.AddDays(20))
    };

    [Fact]
    public async Task Organiser_Can_CreateUpdateAndCancel_Event()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(SampleEvent())
        }.WithSession(cookie);
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<EventResponse>();
        created.Should().NotBeNull();

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/events/{created!.EventId}")
        {
            Content = JsonContent.Create(SampleEvent("Sunrise 5K (Updated)"))
        }.WithSession(cookie);
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<EventResponse>();
        updated!.EventName.Should().Be("Sunrise 5K (Updated)");

        var cancelRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{created.EventId}").WithSession(cookie);
        var cancelResponse = await _client.SendAsync(cancelRequest);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{created.EventId}").WithSession(cookie);
        var getResponse = await _client.SendAsync(getRequest);
        var fetched = await getResponse.Content.ReadFromJsonAsync<EventResponse>();
        fetched!.Status.Should().Be("Cancelled", "cancelling soft-deletes so historical enrolments/results are preserved");
    }

    [Fact]
    public async Task GetEventById_ForMissingEvent_ReturnsNotFound()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/events/999999").WithSession(cookie);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

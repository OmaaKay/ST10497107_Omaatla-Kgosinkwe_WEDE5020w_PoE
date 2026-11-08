using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Enrolments;
using RaceDay.Api.Dtos.EventCategories;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class EnrolmentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EnrolmentsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(int eventId, int categoryId)> CreateEventWithCategoryAsync()
    {
        var organiserCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var eventRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(new EventRequest
            {
                EventName = "Test Half Marathon",
                EventDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(2)),
                StartTime = new TimeOnly(6, 0),
                Location = "Pretoria",
                DistanceKm = 21.1m,
                EventType = EventType.Run,
                MaximumParticipants = 500,
                RegistrationDeadline = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            })
        }.WithSession(organiserCookie);
        var eventResponse = await _client.SendAsync(eventRequest);
        var ev = await eventResponse.Content.ReadFromJsonAsync<EventResponse>();

        var categoryRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{ev!.EventId}/categories")
        {
            Content = JsonContent.Create(new EventCategoryRequest { CategoryName = "Open 18-39" })
        }.WithSession(organiserCookie);
        var categoryResponse = await _client.SendAsync(categoryRequest);
        var category = await categoryResponse.Content.ReadFromJsonAsync<EventCategoryResponse>();

        return (ev.EventId, category!.EventCategoryId);
    }

    [Fact]
    public async Task Participant_Can_Enrol_AndSelectedCategory_IsRecorded()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var participantCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var enrolRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        var enrolResponse = await _client.SendAsync(enrolRequest);

        enrolResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var enrolment = await enrolResponse.Content.ReadFromJsonAsync<EnrolmentResponse>();
        enrolment!.EventCategoryId.Should().Be(categoryId, "the brief requires the selected category to be recorded on the enrolment");
        enrolment.EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task Participant_CannotEnrol_Twice_InSameEvent()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var participantCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var firstRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        (await _client.SendAsync(firstRequest)).StatusCode.Should().Be(HttpStatusCode.Created);

        var secondRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        var secondResponse = await _client.SendAsync(secondRequest);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Organiser_CannotEnrol_InEvent()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var organiserCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(organiserCookie);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Organiser_Can_ViewEnrolments_ForTheirEvent()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var participantCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var enrolRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        await _client.SendAsync(enrolRequest);

        var organiserCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);
        var viewRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{eventId}/enrolments").WithSession(organiserCookie);
        var viewResponse = await _client.SendAsync(viewRequest);

        viewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var enrolments = await viewResponse.Content.ReadFromJsonAsync<List<EnrolmentResponse>>();
        enrolments.Should().ContainSingle(e => e.EventId == eventId);
    }
}

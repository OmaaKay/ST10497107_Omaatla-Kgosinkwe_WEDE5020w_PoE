using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Infrastructure;

/// <summary>Shared helpers for the Organiser pages (load my events, pick the selected one).</summary>
public abstract class OrganiserBase : AppController
{
    protected OrganiserBase(ApiClient api) : base(api) { }

    protected async Task<ApiResult<List<EventDto>>> LoadMyEventsAsync()
    {
        var result = await Api.GetAsync<List<EventDto>>("api/events?mine=true");
        if (result.Success)
        {
            // Cancelled ("deleted") events are hidden from the Organiser's working lists.
            var list = (result.Data ?? new())
                .Where(e => !string.Equals(e.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.EventDate)
                .ToList();
            return new ApiResult<List<EventDto>> { Success = true, StatusCode = result.StatusCode, Data = list };
        }
        return result;
    }

    protected static EventDto? Pick(List<EventDto> events, int? eventId) =>
        events.FirstOrDefault(e => e.EventId == eventId) ?? events.FirstOrDefault();
}

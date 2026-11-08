using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class HomeController : AppController
{
    public HomeController(ApiClient api) : base(api) { }

    /// <summary>
    /// Visitors see the landing page; Participants see all upcoming events with filters;
    /// Organisers go straight to their dashboard.
    /// </summary>
    public async Task<IActionResult> Index(string? q, EventType? type, DateOnly? from, DateOnly? to)
    {
        var session = HttpContext.Session;
        if (!session.IsSignedIn()) return View("Landing");
        if (session.IsOrganiser()) return RedirectToAction("Dashboard", "Organiser");

        var result = await Api.GetAsync<List<EventDto>>("api/events");
        if (Guard(result) is { } failure) return failure;

        var upcoming = (result.Data ?? new()).Where(Display.IsUpcoming).ToList();
        var filtered = upcoming.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            filtered = filtered.Where(e =>
                e.EventName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (e.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (type is not null) filtered = filtered.Where(e => e.EventType == type);
        if (from is not null) filtered = filtered.Where(e => e.EventDate >= from);
        if (to is not null) filtered = filtered.Where(e => e.EventDate <= to);

        return View(new EventListViewModel
        {
            Events = filtered.OrderBy(e => e.EventDate).ToList(),
            TotalUpcoming = upcoming.Count,
            Q = q,
            Type = type,
            From = from,
            To = to
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View();
    }

    /// <summary>Target of UseStatusCodePagesWithReExecute: friendly 404/403/other pages.</summary>
    public IActionResult HttpStatus(int code)
    {
        Response.StatusCode = code;
        return code == 404 ? View("~/Views/Shared/NotFound.cshtml") : View("Error");
    }
}

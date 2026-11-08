using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Organiser)]
public class OrganiserController : OrganiserBase
{
    public OrganiserController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Dashboard()
    {
        var events = await LoadMyEventsAsync();
        if (Guard(events) is { } failure) return failure;

        return View(new DashboardViewModel
        {
            OrganiserName = HttpContext.Session.GetFullName() ?? "Organiser",
            Events = events.Data ?? new()
        });
    }
}

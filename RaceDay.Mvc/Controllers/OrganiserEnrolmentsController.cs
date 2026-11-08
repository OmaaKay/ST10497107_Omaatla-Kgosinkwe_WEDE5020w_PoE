using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Organiser)]
public class OrganiserEnrolmentsController : OrganiserBase
{
    public OrganiserEnrolmentsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index(int? eventId)
    {
        var events = await LoadMyEventsAsync();
        if (Guard(events) is { } failure) return failure;

        var model = new EnrolmentsPageViewModel
        {
            Events = events.Data ?? new()
        };
        model.Selected = Pick(model.Events, eventId);

        if (model.Selected is not null)
        {
            var enrolments = await Api.GetAsync<List<EnrolmentDto>>($"api/events/{model.Selected.EventId}/enrolments");
            if (Guard(enrolments) is { } f2) return f2;
            model.Enrolments = (enrolments.Data ?? new()).OrderBy(e => e.ParticipantName).ToList();
        }

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int eventId, int id, string status)
    {
        var result = await Api.PutAsync($"api/enrolments/{id}/status", new { Status = status });
        if (GuardAuth(result) is { } auth) return auth;

        if (result.Success) TempData["Success"] = $"Enrolment marked as {status}.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { eventId });
    }
}

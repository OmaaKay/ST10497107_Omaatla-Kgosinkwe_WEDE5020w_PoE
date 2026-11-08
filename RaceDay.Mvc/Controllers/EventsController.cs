using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Participant, Roles.Organiser)]
public class EventsController : AppController
{
    public EventsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Details(int id)
    {
        var ev = await Api.GetAsync<EventDto>($"api/events/{id}");
        if (Guard(ev) is { } f1) return f1;

        var categories = await Api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");
        if (Guard(categories) is { } f2) return f2;

        var model = new EventDetailsViewModel
        {
            Event = ev.Data!,
            Categories = (categories.Data ?? new()).Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToList(),
            IsParticipant = HttpContext.Session.IsParticipant()
        };

        if (model.IsParticipant)
        {
            var mine = await Api.GetAsync<List<EnrolmentDto>>("api/enrolments/me");
            if (Guard(mine) is { } f3) return f3;
            model.MyEnrolment = (mine.Data ?? new()).FirstOrDefault(e => e.EventId == id);
        }

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Roles.Participant)]
    public async Task<IActionResult> Enrol(int id, int? categoryId)
    {
        if (categoryId is null)
        {
            TempData["Error"] = "Please choose a category before enrolling.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await Api.PostAsync<EnrolmentDto>($"api/events/{id}/enrolments", new { EventCategoryId = categoryId.Value });
        if (GuardAuth(result) is { } auth) return auth;

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "You're in! Your enrolment has been recorded and is awaiting confirmation by the organiser.";
        return RedirectToAction("Index", "MyEvents");
    }
}

using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Participant)]
public class MyEventsController : AppController
{
    public MyEventsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
    {
        var result = await Api.GetAsync<List<EnrolmentDto>>("api/enrolments/me");
        if (Guard(result) is { } failure) return failure;

        return View((result.Data ?? new()).OrderBy(e => e.EventDate).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await Api.DeleteAsync($"api/enrolments/{id}");
        if (GuardAuth(result) is { } auth) return auth;

        if (result.Success) TempData["Success"] = "Your enrolment has been cancelled.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}

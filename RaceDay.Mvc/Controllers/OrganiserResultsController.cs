using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Organiser)]
public class OrganiserResultsController : OrganiserBase
{
    public OrganiserResultsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index(int? eventId)
    {
        var events = await LoadMyEventsAsync();
        if (Guard(events) is { } failure) return failure;

        var model = new ResultsPageViewModel
        {
            Events = events.Data ?? new()
        };
        model.Selected = Pick(model.Events, eventId);

        if (model.Selected is not null)
        {
            var enrolments = await Api.GetAsync<List<EnrolmentDto>>($"api/events/{model.Selected.EventId}/enrolments");
            if (Guard(enrolments) is { } f2) return f2;

            // Only people who are still in the race can have a result.
            model.Enrolments = (enrolments.Data ?? new())
                .Where(e => !string.Equals(e.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FinishPosition ?? int.MaxValue)
                .ThenBy(e => e.ParticipantName)
                .ToList();
        }

        return View(model);
    }

    /// <summary>Creates the result if the enrolment has none yet, otherwise updates it. Saved results are visible to the Participant straight away.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ResultFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Please enter the finish time as hh:mm:ss." : e.ErrorMessage)
                .Distinct());
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        if (form.FinishPosition is null && form.FinishTime is null)
        {
            TempData["Error"] = "Enter a finishing position, a finish time, or both.";
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        var body = new { form.FinishPosition, form.FinishTime };

        ApiResult result = form.ResultId is null
            ? await Api.PostAsync<ResultDto>($"api/enrolments/{form.EnrolmentId}/result", body)
            : await Api.PutAsync<ResultDto>($"api/results/{form.ResultId}", body);

        if (GuardAuth(result) is { } auth) return auth;

        if (result.Success) TempData["Success"] = "Result saved and published.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { eventId = form.EventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int eventId, int id)
    {
        var result = await Api.DeleteAsync($"api/results/{id}");
        if (GuardAuth(result) is { } auth) return auth;

        if (result.Success) TempData["Success"] = "The result has been removed.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { eventId });
    }
}

/// <summary>Minimal shape of the API's ResultResponse (only used so the typed call can succeed).</summary>
public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int? FinishPosition { get; set; }
    public TimeOnly? FinishTime { get; set; }
}

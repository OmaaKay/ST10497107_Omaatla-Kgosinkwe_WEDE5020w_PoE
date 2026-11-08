using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Organiser)]
public class OrganiserEventsController : OrganiserBase
{
    public OrganiserEventsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
    {
        var events = await LoadMyEventsAsync();
        if (Guard(events) is { } failure) return failure;
        return View(events.Data ?? new());
    }

    [HttpGet]
    public IActionResult Create()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return View("Form", new EventFormViewModel
        {
            EventDate = today.AddDays(30),
            StartTime = new TimeOnly(7, 0),
            RegistrationDeadline = today.AddDays(23),
            EventType = EventType.Run,
            MaximumParticipants = 500
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);

        var created = await Api.PostAsync<EventDto>("api/events", ToRequest(model));
        if (GuardAuth(created) is { } auth) return auth;
        if (!created.Success)
        {
            AddApiErrors(created);
            return View("Form", model);
        }

        if (model.Banner is { Length: > 0 })
        {
            var upload = await Api.UploadAsync<ImageUploadDto>($"api/events/{created.Data!.EventId}/banner", model.Banner);
            if (!upload.Success)
            {
                TempData["Warning"] = $"\"{created.Data.EventName}\" was created, but the banner could not be uploaded: {upload.Error} You can try again by editing the event.";
                return RedirectToAction(nameof(Index));
            }
        }

        TempData["Success"] = $"\"{created.Data!.EventName}\" has been created. Next, add categories so participants can enrol.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await Api.GetAsync<EventDto>($"api/events/{id}");
        if (Guard(result) is { } failure) return failure;

        var e = result.Data!;
        return View("Form", new EventFormViewModel
        {
            EventId = e.EventId,
            EventName = e.EventName,
            Description = e.Description,
            EventDate = e.EventDate,
            StartTime = e.StartTime,
            Location = e.Location,
            DistanceKm = e.DistanceKm,
            EventType = e.EventType,
            MaximumParticipants = e.MaximumParticipants,
            RegistrationDeadline = e.RegistrationDeadline,
            ExistingBannerUrl = e.BannerUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        model.EventId = id;
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);

        var updated = await Api.PutAsync<EventDto>($"api/events/{id}", ToRequest(model));
        if (GuardAuth(updated) is { } auth) return auth;
        if (!updated.Success)
        {
            if (updated.IsNotFound) return Guard(updated)!;
            AddApiErrors(updated);
            return View("Form", model);
        }

        model.ExistingBannerUrl = updated.Data?.BannerUrl;

        if (model.Banner is { Length: > 0 })
        {
            var upload = await Api.UploadAsync<ImageUploadDto>($"api/events/{id}/banner", model.Banner);
            if (!upload.Success)
            {
                TempData["Warning"] = $"The event details were saved, but the banner could not be uploaded: {upload.Error}";
                return RedirectToAction(nameof(Index));
            }
        }

        TempData["Success"] = "The event has been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Api.DeleteAsync($"api/events/{id}");
        if (GuardAuth(result) is { } auth) return auth;

        if (result.Success) TempData["Success"] = "The event has been deleted. Past enrolments and results are kept on record.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------

    private void Validate(EventFormViewModel model)
    {
        if (model.EventDate is not null && model.RegistrationDeadline is not null &&
            model.RegistrationDeadline > model.EventDate)
        {
            ModelState.AddModelError(nameof(model.RegistrationDeadline), "The registration deadline can't be after the event date.");
        }

        var imageProblem = ImageRules.Validate(model.Banner);
        if (imageProblem is not null) ModelState.AddModelError(nameof(model.Banner), imageProblem);
    }

    private static object ToRequest(EventFormViewModel m) => new
    {
        m.EventName,
        m.Description,
        EventDate = m.EventDate!.Value,
        StartTime = m.StartTime!.Value,
        m.Location,
        m.DistanceKm,
        EventType = m.EventType!.Value,
        MaximumParticipants = m.MaximumParticipants!.Value,
        RegistrationDeadline = m.RegistrationDeadline!.Value
    };
}

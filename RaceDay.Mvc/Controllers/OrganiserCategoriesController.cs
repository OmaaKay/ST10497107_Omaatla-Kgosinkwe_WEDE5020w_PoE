using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize(Roles.Organiser)]
public class OrganiserCategoriesController : OrganiserBase
{
    public OrganiserCategoriesController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index(int? eventId)
    {
        var (model, failure) = await BuildPageAsync(eventId, new CategoryFormViewModel());
        return failure ?? View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CategoryFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            var (invalidPage, failure) = await BuildPageAsync(form.EventId, form);
            return failure ?? View("Index", invalidPage);
        }

        var result = await Api.PostAsync<CategoryDto>($"api/events/{form.EventId}/categories",
            new { form.CategoryName, form.Description, form.DistanceKm });
        if (GuardAuth(result) is { } auth) return auth;

        if (!result.Success)
        {
            AddApiErrors(result);
            var (page, failure) = await BuildPageAsync(form.EventId, form);
            return failure ?? View("Index", page);
        }

        TempData["Success"] = $"Category \"{form.CategoryName}\" has been added.";
        return RedirectToAction(nameof(Index), new { eventId = form.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int eventId, int id)
    {
        var categories = await Api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");
        if (Guard(categories) is { } failure) return failure;

        var category = (categories.Data ?? new()).FirstOrDefault(c => c.EventCategoryId == id && c.IsActive);
        if (category is null) return NotFound();

        var ev = await Api.GetAsync<EventDto>($"api/events/{eventId}");
        if (Guard(ev) is { } f2) return f2;
        ViewBag.EventName = ev.Data!.EventName;

        return View(new CategoryFormViewModel
        {
            EventId = eventId,
            CategoryId = id,
            CategoryName = category.CategoryName,
            Description = category.Description,
            DistanceKm = category.DistanceKm
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int eventId, int id, CategoryFormViewModel form)
    {
        form.EventId = eventId;
        form.CategoryId = id;

        if (!ModelState.IsValid) return View(form);

        var result = await Api.PutAsync<CategoryDto>($"api/events/{eventId}/categories/{id}",
            new { form.CategoryName, form.Description, form.DistanceKm });
        if (GuardAuth(result) is { } auth) return auth;

        if (!result.Success)
        {
            AddApiErrors(result);
            return View(form);
        }

        TempData["Success"] = "The category has been updated.";
        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int eventId, int id)
    {
        var result = await Api.DeleteAsync($"api/events/{eventId}/categories/{id}");
        if (GuardAuth(result) is { } auth) return auth;

        if (result.Success) TempData["Success"] = "The category has been removed. Existing enrolments keep their chosen category.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { eventId });
    }

    private async Task<(CategoriesPageViewModel Model, IActionResult? Failure)> BuildPageAsync(int? eventId, CategoryFormViewModel form)
    {
        var model = new CategoriesPageViewModel();

        var events = await LoadMyEventsAsync();
        if (Guard(events) is { } failure) return (model, failure);

        model.Events = events.Data ?? new();
        model.Selected = Pick(model.Events, eventId);
        model.NewCategory = form;

        if (model.Selected is not null)
        {
            model.NewCategory.EventId = model.Selected.EventId;
            var categories = await Api.GetAsync<List<CategoryDto>>($"api/events/{model.Selected.EventId}/categories");
            if (Guard(categories) is { } f2) return (model, f2);
            model.Categories = (categories.Data ?? new()).Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToList();
        }

        return (model, null);
    }
}

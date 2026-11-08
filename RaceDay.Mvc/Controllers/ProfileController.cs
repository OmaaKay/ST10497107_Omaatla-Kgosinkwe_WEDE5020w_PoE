using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[SessionAuthorize] // any signed-in user
public class ProfileController : AppController
{
    public ProfileController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
    {
        var result = await Api.GetAsync<ProfileDto>("api/users/me");
        if (Guard(result) is { } failure) return failure;

        return View(ToViewModel(result.Data!));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return await ReshowAsync(model);

        var result = await Api.PutAsync<ProfileDto>("api/users/me", new
        {
            model.FirstName,
            model.LastName,
            model.PhoneNumber,
            model.DateOfBirth
        });
        if (GuardAuth(result) is { } auth) return auth;

        if (!result.Success)
        {
            AddApiErrors(result);
            return await ReshowAsync(model);
        }

        HttpContext.Session.SetFullName($"{result.Data!.FirstName} {result.Data.LastName}".Trim());
        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        if (picture is null || picture.Length == 0)
        {
            TempData["Error"] = "Please choose an image to upload.";
            return RedirectToAction(nameof(Index));
        }

        var problem = ImageRules.Validate(picture);
        if (problem is not null)
        {
            TempData["Error"] = problem;
            return RedirectToAction(nameof(Index));
        }

        // The file goes to the API, which stores it in Azure Blob Storage. MVC never talks to Azure.
        var result = await Api.UploadAsync<ImageUploadDto>("api/users/me/picture", picture);
        if (GuardAuth(result) is { } auth) return auth;

        if (result.Success)
        {
            HttpContext.Session.SetPicture(result.Data?.Url);
            TempData["Success"] = "Your profile picture has been updated.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReshowAsync(ProfileViewModel posted)
    {
        // Re-load the read-only values (email, role, picture) so the page renders fully.
        var current = await Api.GetAsync<ProfileDto>("api/users/me");
        if (Guard(current) is { } failure) return failure;

        posted.Email = current.Data!.Email;
        posted.Role = current.Data.Role;
        posted.ProfilePictureUrl = current.Data.ProfilePictureUrl;
        return View("Index", posted);
    }

    private static ProfileViewModel ToViewModel(ProfileDto p) => new()
    {
        FirstName = p.FirstName,
        LastName = p.LastName,
        PhoneNumber = p.PhoneNumber,
        DateOfBirth = p.DateOfBirth,
        Email = p.Email,
        Role = p.Role,
        ProfilePictureUrl = p.ProfilePictureUrl
    };
}

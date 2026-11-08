using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Infrastructure;
using RaceDay.Mvc.Models.Api;
using RaceDay.Mvc.Models.ViewModels;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AccountController : AppController
{
    public AccountController(ApiClient api) : base(api) { }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.IsSignedIn()) return RedirectToHome();
        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await Api.PostAsync<AuthDto>("api/auth/register", new
        {
            model.FirstName,
            model.LastName,
            model.Email,
            model.Password,
            model.PhoneNumber,
            model.DateOfBirth,
            model.Role
        });

        if (!result.Success)
        {
            AddApiErrors(result);
            return View(model);
        }

        TempData["Success"] = "Your account has been created. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.IsSignedIn()) return RedirectToHome();
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Start from a clean session so no state from a previous user can leak across.
        HttpContext.Session.Clear();

        var result = await Api.PostAsync<AuthDto>("api/auth/login", new { model.Email, model.Password });
        if (!result.Success || result.Data is null)
        {
            if (result.IsUnauthorized) ModelState.AddModelError(string.Empty, "Incorrect email or password.");
            else AddApiErrors(result);
            model.Password = string.Empty;
            return View(model);
        }

        var user = result.Data;

        // Best effort: fetch the profile picture so it can be shown in the navigation bar.
        string? picture = null;
        var profile = await Api.GetAsync<ProfileDto>("api/users/me");
        if (profile.Success) picture = profile.Data?.ProfilePictureUrl;

        HttpContext.Session.SignIn(user.UserId, user.Role, $"{user.FirstName} {user.LastName}".Trim(), user.Email, picture);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToHome();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (HttpContext.Session.IsSignedIn())
        {
            await Api.PostAsync("api/auth/logout"); // ends the API session; any failure is harmless here
        }

        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private IActionResult RedirectToHome() =>
        HttpContext.Session.IsOrganiser()
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Home");
}

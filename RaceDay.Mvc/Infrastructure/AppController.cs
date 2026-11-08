using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Infrastructure;

/// <summary>Base class for all controllers: shared API client plus consistent handling of API failures.</summary>
public abstract class AppController : Controller
{
    protected AppController(ApiClient api)
    {
        Api = api;
    }

    protected ApiClient Api { get; }

    /// <summary>
    /// Handles authentication/authorisation failures from the API (expired session → login, wrong role → access denied).
    /// Returns null when the result needs no redirect.
    /// </summary>
    protected IActionResult? GuardAuth(ApiResult result)
    {
        if (result.IsUnauthorized)
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        if (result.IsForbidden)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return null;
    }

    /// <summary>For page loads: any failed API call becomes a friendly page instead of a crash.</summary>
    protected IActionResult? Guard(ApiResult result)
    {
        if (result.Success)
        {
            return null;
        }

        var auth = GuardAuth(result);
        if (auth is not null)
        {
            return auth;
        }

        if (result.IsNotFound)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("~/Views/Shared/NotFound.cshtml");
        }

        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View("~/Views/Shared/ApiError.cshtml", result.Error);
    }

    /// <summary>Copies API validation messages into ModelState so the form shows them next to the fields.</summary>
    protected void AddApiErrors(ApiResult result)
    {
        var added = false;
        foreach (var (field, messages) in result.FieldErrors)
        {
            // "$.eventDate"-style JSON paths and unknown names go to the summary.
            var key = field.StartsWith('$') ? string.Empty : field;
            foreach (var message in messages)
            {
                ModelState.AddModelError(key, message);
                added = true;
            }
        }

        if (!added && !string.IsNullOrWhiteSpace(result.Error))
        {
            ModelState.AddModelError(string.Empty, result.Error);
        }
    }
}

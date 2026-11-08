using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Mvc.Infrastructure;

/// <summary>
/// Protects MVC pages using the session. Unauthenticated visitors are redirected to the login page;
/// signed-in users with the wrong role are sent to "Access denied". (The API enforces the same rules
/// server-side, so this is about a clear user experience as well as security.)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IActionFilter
{
    private readonly string[] _roles;

    public SessionAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.IsSignedIn())
        {
            if (context.Controller is Controller c)
            {
                c.TempData["Error"] = "Please log in to continue.";
            }

            var request = context.HttpContext.Request;
            var returnUrl = HttpMethods.IsGet(request.Method) ? request.Path + request.QueryString : null;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }

        var role = session.GetRole();
        if (_roles.Length > 0 && !_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Security;

/// <summary>
/// Returns 401 Unauthorized unless the request carries an active RaceDay
/// session (i.e. the user has logged in via /api/auth/login).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthenticatedAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Session.IsAuthenticated())
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required. Please log in." });
            return;
        }

        await next();
    }
}

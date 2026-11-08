using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Security;

/// <summary>
/// Returns 401 if there is no active session, or 403 if the session's role
/// does not match one of the allowed roles. Use alongside or instead of
/// [RequireAuthenticated] — this attribute checks authentication itself.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _allowedRoles;

    public RequireRoleAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;

        if (!session.IsAuthenticated())
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required. Please log in." });
            return;
        }

        var role = session.GetRole();
        if (role is null || !_allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new { message = $"This action requires role: {string.Join(" or ", _allowedRoles)}." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}

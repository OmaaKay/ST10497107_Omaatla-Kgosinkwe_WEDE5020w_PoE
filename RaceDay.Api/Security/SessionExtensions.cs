using Microsoft.AspNetCore.Http;

namespace RaceDay.Api.Security;

public static class SessionExtensions
{
    public static void SignIn(this ISession session, int userId, string email, string role)
    {
        session.SetInt32(SessionKeys.UserId, userId);
        session.SetString(SessionKeys.Email, email);
        session.SetString(SessionKeys.Role, role);
    }

    public static void SignOut(this ISession session)
    {
        session.Remove(SessionKeys.UserId);
        session.Remove(SessionKeys.Email);
        session.Remove(SessionKeys.Role);
    }

    public static int? GetUserId(this ISession session) => session.GetInt32(SessionKeys.UserId);

    public static string? GetRole(this ISession session) => session.GetString(SessionKeys.Role);

    public static bool IsAuthenticated(this ISession session) => session.GetUserId() is not null;
}

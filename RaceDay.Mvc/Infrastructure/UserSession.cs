namespace RaceDay.Mvc.Infrastructure;

/// <summary>Role names (must match the API's role names).</summary>
public static class Roles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}

/// <summary>Reads and writes the signed-in user's details held in the MVC session.</summary>
public static class UserSession
{
    private const string UserIdKey = "RaceDay_UserId";
    private const string RoleKey = "RaceDay_Role";
    private const string NameKey = "RaceDay_FullName";
    private const string EmailKey = "RaceDay_Email";
    private const string PictureKey = "RaceDay_Picture";

    public static void SignIn(this ISession session, int userId, string role, string fullName, string email, string? pictureUrl)
    {
        session.SetInt32(UserIdKey, userId);
        session.SetString(RoleKey, role);
        session.SetString(NameKey, fullName);
        session.SetString(EmailKey, email);
        session.SetPicture(pictureUrl);
    }

    public static void SetPicture(this ISession session, string? pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(pictureUrl)) session.Remove(PictureKey);
        else session.SetString(PictureKey, pictureUrl);
    }

    public static void SetFullName(this ISession session, string fullName) => session.SetString(NameKey, fullName);

    public static int? GetUserId(this ISession session) => session.GetInt32(UserIdKey);
    public static string? GetRole(this ISession session) => session.GetString(RoleKey);
    public static string? GetFullName(this ISession session) => session.GetString(NameKey);
    public static string? GetEmail(this ISession session) => session.GetString(EmailKey);
    public static string? GetPicture(this ISession session) => session.GetString(PictureKey);
    public static bool IsSignedIn(this ISession session) => session.GetUserId() is not null;
    public static bool IsOrganiser(this ISession session) => session.GetRole() == Roles.Organiser;
    public static bool IsParticipant(this ISession session) => session.GetRole() == Roles.Participant;
}

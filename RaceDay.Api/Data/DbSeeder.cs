using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

/// <summary>
/// Ensures the two roles required by the brief (Participant, Organiser)
/// exist. Roles are never created through the API — only seeded here.
/// </summary>
public static class DbSeeder
{
    public static void SeedRoles(RaceDayDbContext context)
    {
        if (context.Roles.Any())
        {
            return;
        }

        context.Roles.AddRange(
            new Role { RoleName = RoleNames.Participant },
            new Role { RoleName = RoleNames.Organiser }
        );

        context.SaveChanges();
    }
}

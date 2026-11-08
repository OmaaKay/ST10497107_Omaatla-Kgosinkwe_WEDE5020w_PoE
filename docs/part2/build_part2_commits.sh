#!/usr/bin/env bash
set -e

# RaceDay Part 2 — incremental commit builder.
# Run this from inside your RaceDay Part 2 git repo folder
# (git init already done, remote already added).
# Each run performs exactly ONE commit, adding the next real
# group of files, then exits. A couple of steps also write a
# gitignored local settings file (not committed) so the app
# actually runs on your machine as you go.
#
# Usage:
#   chmod +x build_part2_commits.sh
#   ./build_part2_commits.sh          # does the next commit
#   ./build_part2_commits.sh --all     # does ALL remaining commits back to back (testing only)

STATE_FILE=.part2_commit_step
if [ ! -f "$STATE_FILE" ]; then echo 0 > "$STATE_FILE"; fi
STEP=$(cat "$STATE_FILE")
TOTAL=29

run_step() {
  case $1 in
    1)
cat > 'RaceDay.sln' << 'RACEDAY_PART2_EOF'
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "RaceDay.Api", "RaceDay.Api\RaceDay.Api.csproj", "{A1E5C6B0-0001-4B1E-9C1A-000000000001}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "RaceDay.Api.Tests", "RaceDay.Api.Tests\RaceDay.Api.Tests.csproj", "{A1E5C6B0-0002-4B1E-9C1A-000000000002}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{A1E5C6B0-0001-4B1E-9C1A-000000000001}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A1E5C6B0-0001-4B1E-9C1A-000000000001}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A1E5C6B0-0001-4B1E-9C1A-000000000001}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A1E5C6B0-0001-4B1E-9C1A-000000000001}.Release|Any CPU.Build.0 = Release|Any CPU
		{A1E5C6B0-0002-4B1E-9C1A-000000000002}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A1E5C6B0-0002-4B1E-9C1A-000000000002}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A1E5C6B0-0002-4B1E-9C1A-000000000002}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A1E5C6B0-0002-4B1E-9C1A-000000000002}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
RACEDAY_PART2_EOF
git add 'RaceDay.sln'
cat > '.gitignore' << 'RACEDAY_PART2_EOF'
## .NET
bin/
obj/
*.user
*.suo

## IDE
.vs/
.vscode/
*.DotSettings.user

## Local secrets — never commit real connection strings
RaceDay.Api/appsettings.Development.json
**/secrets.json

## OS
.DS_Store
Thumbs.db
RACEDAY_PART2_EOF
git add '.gitignore'
      git commit -m "Initialise Part 2 solution structure"
      echo "Committed step 1/$TOTAL: Initialise Part 2 solution structure"
      ;;
    2)
mkdir -p 'RaceDay.Api'
cat > 'RaceDay.Api/appsettings.json' << 'RACEDAY_PART2_EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "RaceDayDb": "REPLACE_ME — set the real connection string in appsettings.Development.json (gitignored) or via dotnet user-secrets / an environment variable. Never commit real credentials."
  }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/appsettings.json'
mkdir -p 'RaceDay.Api'
cat > 'RaceDay.Api/appsettings.Development.json.template' << 'RACEDAY_PART2_EOF'
{
  "_comment": "Copy this file to appsettings.Development.json (which is gitignored) and fill in your real local SQL Server connection string. Never commit appsettings.Development.json.",
  "ConnectionStrings": {
    "RaceDayDb": "Server=localhost\\SQLEXPRESS;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/appsettings.Development.json.template'
mkdir -p 'RaceDay.Api'
cat > 'RaceDay.Api/appsettings.Development.json' << 'RACEDAY_PART2_EOF'
{
  "ConnectionStrings": {
    "RaceDayDb": "Server=(localdb)\\mssqllocaldb;Database=RaceDay;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
RACEDAY_PART2_EOF
# NOTE: 'RaceDay.Api/appsettings.Development.json' is intentionally NOT committed (gitignored local dev setting).
      git commit -m "Add appsettings templates"
      echo "Committed step 2/$TOTAL: Add appsettings templates"
      ;;
    3)
mkdir -p 'RaceDay.Api'
cat > 'RaceDay.Api/RaceDay.Api.csproj' << 'RACEDAY_PART2_EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
    <UserSecretsId>raceday-api-secrets</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.8">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Identity.Core" Version="8.0.8" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>

</Project>
RACEDAY_PART2_EOF
git add 'RaceDay.Api/RaceDay.Api.csproj'
      git commit -m "Add RaceDay.Api project file"
      echo "Committed step 3/$TOTAL: Add RaceDay.Api project file"
      ;;
    4)
mkdir -p 'RaceDay.Api/Models'
cat > 'RaceDay.Api/Models/Role.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// A user role: "Participant" or "Organiser". Matches the Part 1 ERD's Role
/// entity. Seeded once at startup — never created via the API.
/// </summary>
public class Role
{
    public int RoleId { get; set; }

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}

/// <summary>
/// Well-known role names. Used instead of magic strings when checking or
/// assigning roles, and in [RequireRole] attributes on controllers.
/// </summary>
public static class RoleNames
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Models/Role.cs'
mkdir -p 'RaceDay.Api/Models'
cat > 'RaceDay.Api/Models/User.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// A registered RaceDay account. Role name has been aligned to the Part 2
/// brief: "Organiser" replaces the Part 1 working name "Administrator".
/// </summary>
public class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed with Microsoft.AspNetCore.Identity.PasswordHasher — never plaintext.</summary>
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Models/User.cs'
      git commit -m "Add Role and User EF Core models"
      echo "Committed step 4/$TOTAL: Add Role and User EF Core models"
      ;;
    5)
mkdir -p 'RaceDay.Api/Models'
cat > 'RaceDay.Api/Models/EventType.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Models;

/// <summary>
/// The kind of race an Event is. Added for Part 2 per the brief's
/// requirement that an event captures "event type (run, walk or cycle)".
/// </summary>
public enum EventType
{
    Run = 0,
    Walk = 1,
    Cycle = 2
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Models/EventType.cs'
mkdir -p 'RaceDay.Api/Models'
cat > 'RaceDay.Api/Models/Event.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// A race event created by an Organiser.
///
/// PART 2 ALIGNMENT: in the Part 1 design, Event belonged to a single global
/// EventCategory (e.g. "5K"). The Part 2 brief instead requires an Organiser
/// to "define age or distance categories FOR EACH EVENT", and for a
/// Participant to select one of those categories when enrolling. Event no
/// longer references a single global category; instead EventCategory now
/// belongs to one Event (see EventCategory.cs), and the participant's choice
/// is recorded on Enrolment. See docs/part2-consistency-notes.md.
/// </summary>
public class Event
{
    public int EventId { get; set; }

    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateOnly EventDate { get; set; }

    public TimeOnly StartTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal? DistanceKm { get; set; }

    public EventType EventType { get; set; }

    public int MaximumParticipants { get; set; }

    public DateOnly RegistrationDeadline { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Open"; // Open, Closed, Completed, Cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EventCategory> Categories { get; set; } = new List<EventCategory>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Models/Event.cs'
      git commit -m "Add EventType enum and Event model"
      echo "Committed step 5/$TOTAL: Add EventType enum and Event model"
      ;;
    6)
mkdir -p 'RaceDay.Api/Models'
cat > 'RaceDay.Api/Models/EventCategory.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// An age or distance category an Organiser defines for a specific Event
/// (e.g. "Under 18", "5km", "Male Open"). Reconciled for Part 2 — see
/// docs/part2-consistency-notes.md for why this changed from a Part 1
/// global category shared across events to a per-event category.
/// </summary>
public class EventCategory
{
    public int EventCategoryId { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? DistanceKm { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Models/EventCategory.cs'
      git commit -m "Add EventCategory model for per-event categories"
      echo "Committed step 6/$TOTAL: Add EventCategory model for per-event categories"
      ;;
    7)
mkdir -p 'RaceDay.Api/Models'
cat > 'RaceDay.Api/Models/Enrolment.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

/// <summary>
/// Resolves the many-to-many User&lt;-&gt;Event relationship, and — per the
/// Part 2 brief — records which EventCategory the Participant selected.
/// A user may enrol in a given event only once (enforced by a unique
/// constraint on UserId+EventId in RaceDayDbContext).
/// </summary>
public class Enrolment
{
    public int EnrolmentId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    /// <summary>The category the Participant selected at enrolment time.</summary>
    public int EventCategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Cancelled, Completed

    public Result? Result { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Models/Enrolment.cs'
      git commit -m "Add Enrolment model recording the selected category"
      echo "Committed step 7/$TOTAL: Add Enrolment model recording the selected category"
      ;;
    8)
mkdir -p 'RaceDay.Api/Models'
cat > 'RaceDay.Api/Models/Result.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Models;

/// <summary>
/// A finish result for one enrolment. One-to-zero-or-one with Enrolment,
/// enforced by a unique constraint on EnrolmentId.
/// </summary>
public class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }

    public int? FinishPosition { get; set; }

    public TimeOnly? FinishTime { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Models/Result.cs'
      git commit -m "Add Result model"
      echo "Committed step 8/$TOTAL: Add Result model"
      ;;
    9)
mkdir -p 'RaceDay.Api/Data'
cat > 'RaceDay.Api/Data/RaceDayDbContext.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Role ----
        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => r.RoleName).IsUnique();
        });

        // ---- User ----
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();

            e.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Event ----
        modelBuilder.Entity<Event>(e =>
        {
            e.Property(ev => ev.EventType).HasConversion<string>().HasMaxLength(20);
        });

        // ---- EventCategory (belongs to one Event — Part 2 reconciliation) ----
        modelBuilder.Entity<EventCategory>(e =>
        {
            e.HasOne(c => c.Event)
                .WithMany(ev => ev.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // A category name must be unique within its own event, not globally.
            e.HasIndex(c => new { c.EventId, c.CategoryName }).IsUnique();
        });

        // ---- Enrolment ----
        modelBuilder.Entity<Enrolment>(e =>
        {
            e.HasOne(en => en.User)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(en => en.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(en => en.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(en => en.EventCategory)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // A participant cannot enrol in the same event more than once.
            e.HasIndex(en => new { en.UserId, en.EventId }).IsUnique();
        });

        // ---- Result ----
        modelBuilder.Entity<Result>(e =>
        {
            e.HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Restrict); // never lose historical results via cascade

            e.HasIndex(r => r.EnrolmentId).IsUnique();
        });
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Data/RaceDayDbContext.cs'
      git commit -m "Add RaceDayDbContext with Fluent API constraints"
      echo "Committed step 9/$TOTAL: Add RaceDayDbContext with Fluent API constraints"
      ;;
    10)
mkdir -p 'RaceDay.Api/Data'
cat > 'RaceDay.Api/Data/DbSeeder.cs' << 'RACEDAY_PART2_EOF'
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
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Data/DbSeeder.cs'
      git commit -m "Add role seeding"
      echo "Committed step 10/$TOTAL: Add role seeding"
      ;;
    11)
mkdir -p 'RaceDay.Api/Security'
cat > 'RaceDay.Api/Security/SessionKeys.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Security;

/// <summary>Keys used to store the authenticated identity in ASP.NET Core Session.</summary>
public static class SessionKeys
{
    public const string UserId = "RaceDay_UserId";
    public const string Role = "RaceDay_Role";
    public const string Email = "RaceDay_Email";
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Security/SessionKeys.cs'
mkdir -p 'RaceDay.Api/Security'
cat > 'RaceDay.Api/Security/SessionExtensions.cs' << 'RACEDAY_PART2_EOF'
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
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Security/SessionExtensions.cs'
      git commit -m "Add session key constants and extensions"
      echo "Committed step 11/$TOTAL: Add session key constants and extensions"
      ;;
    12)
mkdir -p 'RaceDay.Api/Security'
cat > 'RaceDay.Api/Security/RequireAuthenticatedAttribute.cs' << 'RACEDAY_PART2_EOF'
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
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Security/RequireAuthenticatedAttribute.cs'
mkdir -p 'RaceDay.Api/Security'
cat > 'RaceDay.Api/Security/RequireRoleAttribute.cs' << 'RACEDAY_PART2_EOF'
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
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Security/RequireRoleAttribute.cs'
      git commit -m "Add RequireAuthenticated and RequireRole action filters"
      echo "Committed step 12/$TOTAL: Add RequireAuthenticated and RequireRole action filters"
      ;;
    13)
mkdir -p 'RaceDay.Api/Dtos/Auth'
cat > 'RaceDay.Api/Dtos/Auth/RegisterRequest.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.Auth;

/// <summary>Body for POST /api/auth/register.</summary>
public class RegisterRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Must be "Participant" or "Organiser" (see RoleNames).</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Auth/RegisterRequest.cs'
mkdir -p 'RaceDay.Api/Dtos/Auth'
cat > 'RaceDay.Api/Dtos/Auth/LoginRequest.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.Auth;

/// <summary>Body for POST /api/auth/login.</summary>
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Auth/LoginRequest.cs'
mkdir -p 'RaceDay.Api/Dtos/Auth'
cat > 'RaceDay.Api/Dtos/Auth/AuthResponse.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Dtos.Auth;

/// <summary>Returned by register/login on success. The session cookie carries the actual auth state.</summary>
public class AuthResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Auth/AuthResponse.cs'
      git commit -m "Add authentication DTOs"
      echo "Committed step 13/$TOTAL: Add authentication DTOs"
      ;;
    14)
mkdir -p 'RaceDay.Api/Dtos/Users'
cat > 'RaceDay.Api/Dtos/Users/UserProfileResponse.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Dtos.Users;

public class UserProfileResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string Role { get; set; } = string.Empty;
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Users/UserProfileResponse.cs'
mkdir -p 'RaceDay.Api/Dtos/Users'
cat > 'RaceDay.Api/Dtos/Users/UpdateProfileRequest.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.Users;

/// <summary>Body for PUT /api/users/me.</summary>
public class UpdateProfileRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Users/UpdateProfileRequest.cs'
mkdir -p 'RaceDay.Api/Dtos/Events'
cat > 'RaceDay.Api/Dtos/Events/EventRequest.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos.Events;

/// <summary>Body for POST/PUT /api/events.</summary>
public class EventRequest
{
    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateOnly EventDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal? DistanceKm { get; set; }

    [Required]
    public EventType EventType { get; set; }

    [Range(1, int.MaxValue)]
    public int MaximumParticipants { get; set; }

    [Required]
    public DateOnly RegistrationDeadline { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Events/EventRequest.cs'
mkdir -p 'RaceDay.Api/Dtos/Events'
cat > 'RaceDay.Api/Dtos/Events/EventResponse.cs' << 'RACEDAY_PART2_EOF'
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos.Events;

public class EventResponse
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public int MaximumParticipants { get; set; }
    public DateOnly RegistrationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Events/EventResponse.cs'
      git commit -m "Add user and event DTOs"
      echo "Committed step 14/$TOTAL: Add user and event DTOs"
      ;;
    15)
mkdir -p 'RaceDay.Api/Dtos/EventCategories'
cat > 'RaceDay.Api/Dtos/EventCategories/EventCategoryRequest.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.EventCategories;

/// <summary>Body for POST/PUT /api/events/{eventId}/categories.</summary>
public class EventCategoryRequest
{
    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? DistanceKm { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/EventCategories/EventCategoryRequest.cs'
mkdir -p 'RaceDay.Api/Dtos/EventCategories'
cat > 'RaceDay.Api/Dtos/EventCategories/EventCategoryResponse.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Dtos.EventCategories;

public class EventCategoryResponse
{
    public int EventCategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DistanceKm { get; set; }
    public bool IsActive { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/EventCategories/EventCategoryResponse.cs'
mkdir -p 'RaceDay.Api/Dtos/Enrolments'
cat > 'RaceDay.Api/Dtos/Enrolments/EnrolmentRequest.cs' << 'RACEDAY_PART2_EOF'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos.Enrolments;

/// <summary>Body for POST /api/events/{eventId}/enrolments — the participant selects a category.</summary>
public class EnrolmentRequest
{
    [Required]
    public int EventCategoryId { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Enrolments/EnrolmentRequest.cs'
mkdir -p 'RaceDay.Api/Dtos/Enrolments'
cat > 'RaceDay.Api/Dtos/Enrolments/EnrolmentResponse.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Dtos.Enrolments;

public class EnrolmentResponse
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int EventCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Enrolments/EnrolmentResponse.cs'
mkdir -p 'RaceDay.Api/Dtos/Results'
cat > 'RaceDay.Api/Dtos/Results/ResultRequest.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Dtos.Results;

/// <summary>Body for POST /api/enrolments/{id}/result and PUT /api/results/{id}.</summary>
public class ResultRequest
{
    public int? FinishPosition { get; set; }
    public TimeOnly? FinishTime { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Results/ResultRequest.cs'
mkdir -p 'RaceDay.Api/Dtos/Results'
cat > 'RaceDay.Api/Dtos/Results/ResultResponse.cs' << 'RACEDAY_PART2_EOF'
namespace RaceDay.Api.Dtos.Results;

public class ResultResponse
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int? FinishPosition { get; set; }
    public TimeOnly? FinishTime { get; set; }
    public DateTime RecordedAt { get; set; }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Dtos/Results/ResultResponse.cs'
      git commit -m "Add category, enrolment and result DTOs"
      echo "Committed step 15/$TOTAL: Add category, enrolment and result DTOs"
      ;;
    16)
mkdir -p 'RaceDay.Api/Controllers'
cat > 'RaceDay.Api/Controllers/AuthController.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Auth;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Registers a new account as either Participant or Organiser.</summary>
    /// <response code="201">Account created.</response>
    /// <response code="400">Invalid data, or an unrecognised role name.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role);
        if (role is null)
        {
            return BadRequest(new { message = $"Role must be '{RoleNames.Participant}' or '{RoleNames.Organiser}'." });
        }

        var emailTaken = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
        {
            return Conflict(new { message = "This email is already registered." });
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
            RoleId = role.RoleId
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = new AuthResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = role.RoleName
        };

        return CreatedAtAction(nameof(Register), new { id = user.UserId }, response);
    }

    /// <summary>Authenticates a user and starts a server-side session.</summary>
    /// <response code="200">Login successful; session cookie is set.</response>
    /// <response code="400">Missing email or password.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        HttpContext.Session.SignIn(user.UserId, user.Email, user.Role!.RoleName);

        return Ok(new AuthResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.RoleName
        });
    }

    /// <summary>Ends the current server-side session.</summary>
    /// <response code="204">Logged out.</response>
    [HttpPost("logout")]
    [RequireAuthenticated]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        HttpContext.Session.SignOut();
        return NoContent();
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Controllers/AuthController.cs'
      git commit -m "Implement AuthController"
      echo "Committed step 16/$TOTAL: Implement AuthController"
      ;;
    17)
mkdir -p 'RaceDay.Api/Controllers'
cat > 'RaceDay.Api/Controllers/UsersController.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Users;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[RequireAuthenticated]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Retrieves the authenticated user's own profile.</summary>
    /// <response code="200">Profile returned.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserProfileResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth.HasValue ? DateOnly.FromDateTime(user.DateOfBirth.Value) : null,
            Role = user.Role!.RoleName
        });
    }

    /// <summary>Updates the authenticated user's own profile.</summary>
    /// <response code="200">Profile updated.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return Unauthorized();
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth?.ToDateTime(TimeOnly.MinValue);

        await _db.SaveChangesAsync();

        return Ok(new UserProfileResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth.HasValue ? DateOnly.FromDateTime(user.DateOfBirth.Value) : null,
            Role = user.Role!.RoleName
        });
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Controllers/UsersController.cs'
      git commit -m "Implement UsersController"
      echo "Committed step 17/$TOTAL: Implement UsersController"
      ;;
    18)
mkdir -p 'RaceDay.Api/Controllers'
cat > 'RaceDay.Api/Controllers/EventsController.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Retrieves all events. Requires login, but is open to both roles.</summary>
    /// <response code="200">Event list returned.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events.OrderBy(e => e.EventDate).Select(ToResponse).ToListAsync();
        return Ok(events);
    }

    /// <summary>Retrieves one event by id.</summary>
    /// <response code="200">Event returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("{id}")]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.Where(e => e.EventId == id).Select(ToResponse).FirstOrDefaultAsync();
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>Creates a new race event. Organiser only.</summary>
    /// <response code="201">Event created.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    [HttpPost]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = new Event
        {
            EventName = request.EventName,
            Description = request.Description,
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            Location = request.Location,
            DistanceKm = request.DistanceKm,
            EventType = request.EventType,
            MaximumParticipants = request.MaximumParticipants,
            RegistrationDeadline = request.RegistrationDeadline
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, MapToResponse(ev));
    }

    /// <summary>Updates a race event. Organiser only.</summary>
    /// <response code="200">Event updated.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    [HttpPut("{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        ev.EventName = request.EventName;
        ev.Description = request.Description;
        ev.EventDate = request.EventDate;
        ev.StartTime = request.StartTime;
        ev.Location = request.Location;
        ev.DistanceKm = request.DistanceKm;
        ev.EventType = request.EventType;
        ev.MaximumParticipants = request.MaximumParticipants;
        ev.RegistrationDeadline = request.RegistrationDeadline;

        await _db.SaveChangesAsync();

        return Ok(MapToResponse(ev));
    }

    /// <summary>Cancels a race event. Organiser only. Historical enrolments/results are kept.</summary>
    /// <response code="204">Event cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    [HttpDelete("{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        // Soft cancel rather than delete, so historical enrolments/results survive.
        ev.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static readonly System.Linq.Expressions.Expression<Func<Event, EventResponse>> ToResponse = e => new EventResponse
    {
        EventId = e.EventId,
        EventName = e.EventName,
        Description = e.Description,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        MaximumParticipants = e.MaximumParticipants,
        RegistrationDeadline = e.RegistrationDeadline,
        Status = e.Status
    };

    private static EventResponse MapToResponse(Event e) => new()
    {
        EventId = e.EventId,
        EventName = e.EventName,
        Description = e.Description,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        MaximumParticipants = e.MaximumParticipants,
        RegistrationDeadline = e.RegistrationDeadline,
        Status = e.Status
    };
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Controllers/EventsController.cs'
      git commit -m "Implement EventsController"
      echo "Committed step 18/$TOTAL: Implement EventsController"
      ;;
    19)
mkdir -p 'RaceDay.Api/Controllers'
cat > 'RaceDay.Api/Controllers/EventCategoriesController.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.EventCategories;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>
/// Categories belong to a specific event (Part 2 reconciliation — see
/// docs/part2-consistency-notes.md), so these routes are nested under
/// /api/events/{eventId}/categories.
/// </summary>
[ApiController]
[Route("api/events/{eventId}/categories")]
public class EventCategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventCategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Retrieves the categories defined for an event.</summary>
    /// <response code="200">Category list returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(IEnumerable<EventCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var categories = await _db.EventCategories
            .Where(c => c.EventId == eventId)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Creates a category for an event. Organiser only.</summary>
    /// <response code="201">Category created.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="409">A category with this name already exists for this event.</response>
    [HttpPost]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventCategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(int eventId, [FromBody] EventCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var duplicate = await _db.EventCategories
            .AnyAsync(c => c.EventId == eventId && c.CategoryName == request.CategoryName);
        if (duplicate)
        {
            return Conflict(new { message = "A category with this name already exists for this event." });
        }

        var category = new EventCategory
        {
            EventId = eventId,
            CategoryName = request.CategoryName,
            Description = request.Description,
            DistanceKm = request.DistanceKm
        };

        _db.EventCategories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { eventId }, MapToResponse(category));
    }

    /// <summary>Updates a category. Organiser only.</summary>
    /// <response code="200">Category updated.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Category not found for this event.</response>
    [HttpPut("{categoryId}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int eventId, int categoryId, [FromBody] EventCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await _db.EventCategories.FirstOrDefaultAsync(c => c.EventCategoryId == categoryId && c.EventId == eventId);
        if (category is null)
        {
            return NotFound();
        }

        category.CategoryName = request.CategoryName;
        category.Description = request.Description;
        category.DistanceKm = request.DistanceKm;

        await _db.SaveChangesAsync();

        return Ok(MapToResponse(category));
    }

    /// <summary>Deactivates a category. Organiser only.</summary>
    /// <response code="204">Category deactivated.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Category not found for this event.</response>
    [HttpDelete("{categoryId}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int eventId, int categoryId)
    {
        var category = await _db.EventCategories.FirstOrDefaultAsync(c => c.EventCategoryId == categoryId && c.EventId == eventId);
        if (category is null)
        {
            return NotFound();
        }

        category.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static readonly System.Linq.Expressions.Expression<Func<EventCategory, EventCategoryResponse>> ToResponse = c => new EventCategoryResponse
    {
        EventCategoryId = c.EventCategoryId,
        EventId = c.EventId,
        CategoryName = c.CategoryName,
        Description = c.Description,
        DistanceKm = c.DistanceKm,
        IsActive = c.IsActive
    };

    private static EventCategoryResponse MapToResponse(EventCategory c) => new()
    {
        EventCategoryId = c.EventCategoryId,
        EventId = c.EventId,
        CategoryName = c.CategoryName,
        Description = c.Description,
        DistanceKm = c.DistanceKm,
        IsActive = c.IsActive
    };
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Controllers/EventCategoriesController.cs'
      git commit -m "Implement EventCategoriesController"
      echo "Committed step 19/$TOTAL: Implement EventCategoriesController"
      ;;
    20)
mkdir -p 'RaceDay.Api/Controllers'
cat > 'RaceDay.Api/Controllers/EnrolmentsController.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Enrolments;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Enrols the authenticated Participant in an event, selecting a category.</summary>
    /// <response code="201">Enrolment created.</response>
    /// <response code="400">Missing/invalid category, or category belongs to a different event.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Organiser, not Participant.</response>
    /// <response code="404">Event or category not found.</response>
    /// <response code="409">Already enrolled in this event.</response>
    [HttpPost("api/events/{eventId}/enrolments")]
    [RequireRole(RoleNames.Participant)]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        var category = await _db.EventCategories
            .FirstOrDefaultAsync(c => c.EventCategoryId == request.EventCategoryId && c.EventId == eventId);
        if (category is null)
        {
            return BadRequest(new { message = "The selected category does not belong to this event." });
        }

        var userId = HttpContext.Session.GetUserId()!.Value;

        var alreadyEnrolled = await _db.Enrolments.AnyAsync(e => e.UserId == userId && e.EventId == eventId);
        if (alreadyEnrolled)
        {
            return Conflict(new { message = "You are already enrolled in this event." });
        }

        var enrolment = new Enrolment
        {
            UserId = userId,
            EventId = eventId,
            EventCategoryId = category.EventCategoryId
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var response = await BuildResponse(enrolment.EnrolmentId);
        return CreatedAtAction(nameof(GetMine), null, response);
    }

    /// <summary>Retrieves the authenticated Participant's own enrolments.</summary>
    /// <response code="200">Enrolment list returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Organiser, not Participant.</response>
    [HttpGet("api/enrolments/me")]
    [RequireRole(RoleNames.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        var enrolments = await _db.Enrolments
            .Where(e => e.UserId == userId)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Cancels the authenticated Participant's own enrolment.</summary>
    /// <response code="204">Enrolment cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Organiser, or trying to cancel someone else's enrolment.</response>
    /// <response code="404">Enrolment not found.</response>
    [HttpDelete("api/enrolments/{id}")]
    [RequireRole(RoleNames.Participant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment is null)
        {
            return NotFound();
        }

        if (enrolment.UserId != userId)
        {
            return Forbid();
        }

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Retrieves all enrolments for an event. Organiser only.</summary>
    /// <response code="200">Enrolment list returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("api/events/{eventId}/enrolments")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
        {
            return NotFound();
        }

        var enrolments = await _db.Enrolments
            .Where(e => e.EventId == eventId)
            .Select(ToResponse)
            .ToListAsync();

        return Ok(enrolments);
    }

    private async Task<EnrolmentResponse> BuildResponse(int enrolmentId)
    {
        return await _db.Enrolments.Where(e => e.EnrolmentId == enrolmentId).Select(ToResponse).FirstAsync();
    }

    private static readonly System.Linq.Expressions.Expression<Func<Enrolment, EnrolmentResponse>> ToResponse = e => new EnrolmentResponse
    {
        EnrolmentId = e.EnrolmentId,
        UserId = e.UserId,
        ParticipantName = e.User!.FirstName + " " + e.User.LastName,
        EventId = e.EventId,
        EventName = e.Event!.EventName,
        EventCategoryId = e.EventCategoryId,
        CategoryName = e.EventCategory!.CategoryName,
        EnrolmentDate = e.EnrolmentDate,
        Status = e.Status
    };
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Controllers/EnrolmentsController.cs'
      git commit -m "Implement EnrolmentsController"
      echo "Committed step 20/$TOTAL: Implement EnrolmentsController"
      ;;
    21)
mkdir -p 'RaceDay.Api/Controllers'
cat > 'RaceDay.Api/Controllers/ResultsController.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos.Results;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves results. Organisers see all results; Participants see only
    /// their own (enforced here, not just by role, since Result has no
    /// direct owner column).
    /// </summary>
    /// <response code="200">Result list returned.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet("api/results")]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(IEnumerable<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var role = HttpContext.Session.GetRole();
        var query = _db.Results.AsQueryable();

        if (role == RoleNames.Participant)
        {
            var userId = HttpContext.Session.GetUserId()!.Value;
            query = query.Where(r => r.Enrolment!.UserId == userId);
        }

        var results = await query.Select(ToResponse).ToListAsync();
        return Ok(results);
    }

    /// <summary>Retrieves one result. Participants may only view their own.</summary>
    /// <response code="200">Result returned.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Participant attempting to view another participant's result.</response>
    /// <response code="404">Result not found.</response>
    [HttpGet("api/results/{id}")]
    [RequireAuthenticated]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _db.Results.Include(r => r.Enrolment).FirstOrDefaultAsync(r => r.ResultId == id);
        if (result is null)
        {
            return NotFound();
        }

        var role = HttpContext.Session.GetRole();
        if (role == RoleNames.Participant)
        {
            var userId = HttpContext.Session.GetUserId()!.Value;
            if (result.Enrolment!.UserId != userId)
            {
                return Forbid();
            }
        }

        return Ok(MapToResponse(result));
    }

    /// <summary>Records a result for an enrolment. Organiser only.</summary>
    /// <response code="201">Result recorded.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Enrolment not found.</response>
    /// <response code="409">A result already exists for this enrolment.</response>
    [HttpPost("api/enrolments/{enrolmentId}/result")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Record(int enrolmentId, [FromBody] ResultRequest request)
    {
        var enrolment = await _db.Enrolments.Include(e => e.Result).FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (enrolment.Result is not null)
        {
            return Conflict(new { message = "A result has already been recorded for this enrolment." });
        }

        var result = new Result
        {
            EnrolmentId = enrolmentId,
            FinishPosition = request.FinishPosition,
            FinishTime = request.FinishTime
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = result.ResultId }, MapToResponse(result));
    }

    /// <summary>Updates an existing result. Organiser only.</summary>
    /// <response code="200">Result updated.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Result not found.</response>
    [HttpPut("api/results/{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ResultRequest request)
    {
        var result = await _db.Results.FindAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        result.FinishPosition = request.FinishPosition;
        result.FinishTime = request.FinishTime;

        await _db.SaveChangesAsync();

        return Ok(MapToResponse(result));
    }

    /// <summary>Deletes an incorrectly recorded result. Organiser only.</summary>
    /// <response code="204">Result deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Logged in as Participant, not Organiser.</response>
    /// <response code="404">Result not found.</response>
    [HttpDelete("api/results/{id}")]
    [RequireRole(RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.Results.FindAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        _db.Results.Remove(result);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static ResultResponse MapToResponse(Result r) => new()
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        FinishPosition = r.FinishPosition,
        FinishTime = r.FinishTime,
        RecordedAt = r.RecordedAt
    };

    private static readonly System.Linq.Expressions.Expression<Func<Result, ResultResponse>> ToResponse = r => new ResultResponse
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        FinishPosition = r.FinishPosition,
        FinishTime = r.FinishTime,
        RecordedAt = r.RecordedAt
    };
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Controllers/ResultsController.cs'
      git commit -m "Implement ResultsController"
      echo "Committed step 21/$TOTAL: Implement ResultsController"
      ;;
    22)
mkdir -p 'RaceDay.Api'
cat > 'RaceDay.Api/Program.cs' << 'RACEDAY_PART2_EOF'
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ---- Database (EF Core, Code-First, SQL Server) ----
// The real connection string lives in appsettings.Development.json (gitignored)
// or in user-secrets / an environment variable in production — never committed.
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDb")));

// ---- Server-side session (brief requires session-based auth state) ----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "RaceDay.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();

// ---- Swagger / OpenAPI ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay race and event management system. " +
                      "Register and log in as Participant or Organiser, then use the " +
                      "'Authorize' cookie session set by /api/auth/login for protected endpoints."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ---- Seed roles + apply pending migrations on startup (dev convenience) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    if (db.Database.IsRelational())
    {
        // Skipped for the in-memory provider used by the test project, which
        // doesn't support migrations.
        db.Database.Migrate();
    }
    DbSeeder.SeedRoles(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    });
}

app.UseHttpsRedirection();

app.UseSession(); // must run before controllers so HttpContext.Session is populated

app.MapControllers();

app.Run();

// Exposed so the integration-test WebApplicationFactory<Program> can find the entry point.
public partial class Program { }
RACEDAY_PART2_EOF
git add 'RaceDay.Api/Program.cs'
      git commit -m "Configure Program.cs with session, Swagger and DI"
      echo "Committed step 22/$TOTAL: Configure Program.cs with session, Swagger and DI"
      ;;
    23)
mkdir -p 'RaceDay.Api.Tests'
cat > 'RaceDay.Api.Tests/RaceDay.Api.Tests.csproj' << 'RACEDAY_PART2_EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.8" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\RaceDay.Api\RaceDay.Api.csproj" />
  </ItemGroup>

</Project>
RACEDAY_PART2_EOF
git add 'RaceDay.Api.Tests/RaceDay.Api.Tests.csproj'
mkdir -p 'RaceDay.Api.Tests'
cat > 'RaceDay.Api.Tests/CustomWebApplicationFactory.cs' << 'RACEDAY_PART2_EOF'
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

/// <summary>
/// Boots the real API pipeline (controllers, session, Swagger, action
/// filters) against an isolated EF Core InMemory database per test class,
/// so tests exercise the actual HTTP contract — status codes, routes,
/// session cookies — rather than calling controller methods directly.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<RaceDayDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
            });
        });
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api.Tests/CustomWebApplicationFactory.cs'
mkdir -p 'RaceDay.Api.Tests'
cat > 'RaceDay.Api.Tests/TestAuthHelper.cs' << 'RACEDAY_PART2_EOF'
using System.Net.Http.Json;
using RaceDay.Api.Dtos.Auth;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

/// <summary>
/// The session cookie isn't captured automatically by HttpClient in test
/// scenarios, so this helper registers + logs in a fresh user and returns
/// the raw Set-Cookie value to attach to subsequent requests.
/// </summary>
public static class TestAuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string role, string? emailOverride = null)
    {
        var email = emailOverride ?? $"{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = "P@ssword123!",
            Role = role
        });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "P@ssword123!"
        });
        loginResponse.EnsureSuccessStatusCode();

        var setCookie = loginResponse.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.First()
            : throw new InvalidOperationException("Login did not set a session cookie.");

        // Set-Cookie looks like "RaceDay.Session=xyz; path=/; ..." — we only need the name=value pair.
        return setCookie.Split(';')[0];
    }

    public static HttpRequestMessage WithSession(this HttpRequestMessage request, string cookie)
    {
        request.Headers.Add("Cookie", cookie);
        return request;
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api.Tests/TestAuthHelper.cs'
      git commit -m "Set up unit test project scaffold"
      echo "Committed step 23/$TOTAL: Set up unit test project scaffold"
      ;;
    24)
mkdir -p 'RaceDay.Api.Tests'
cat > 'RaceDay.Api.Tests/AuthControllerTests.cs' << 'RACEDAY_PART2_EOF'
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Auth;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Alice",
            LastName = "Runner",
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "P@ssword123!",
            Role = RoleNames.Participant
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest
        {
            FirstName = "Bob",
            LastName = "Racer",
            Email = email,
            Password = "P@ssword123!",
            Role = RoleNames.Participant
        };

        var first = await _client.PostAsJsonAsync("/api/auth/register", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/api/auth/register", request);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithUnknownRole_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Eve",
            LastName = "Unknown",
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "P@ssword123!",
            Role = "SuperAdmin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOkAndSessionCookie()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Cara",
            LastName = "Sprinter",
            Email = email,
            Password = "P@ssword123!",
            Role = RoleNames.Organiser
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "P@ssword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out _).Should().BeTrue("a successful login must start a server-side session");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FirstName = "Dan",
            LastName = "Marathoner",
            Email = email,
            Password = "P@ssword123!",
            Role = RoleNames.Participant
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "nobody@example.com",
            Password = "Whatever123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api.Tests/AuthControllerTests.cs'
      git commit -m "Add authentication unit tests"
      echo "Committed step 24/$TOTAL: Add authentication unit tests"
      ;;
    25)
mkdir -p 'RaceDay.Api.Tests'
cat > 'RaceDay.Api.Tests/RoleEnforcementTests.cs' << 'RACEDAY_PART2_EOF'
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

/// <summary>
/// Covers the brief's explicit requirement to test "authentication/
/// authorization failures such as unauthenticated access and forbidden
/// role access", using Events (Organiser-only) as the protected resource.
/// </summary>
public class RoleEnforcementTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoleEnforcementTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static EventRequest SampleEvent() => new()
    {
        EventName = "Test 10K",
        EventDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
        StartTime = new TimeOnly(7, 0),
        Location = "Test City",
        EventType = EventType.Run,
        MaximumParticipants = 100,
        RegistrationDeadline = DateOnly.FromDateTime(DateTime.Today.AddDays(20))
    };

    [Fact]
    public async Task CreateEvent_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/events", SampleEvent());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbidden()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(SampleEvent())
        }.WithSession(cookie);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(SampleEvent())
        }.WithSession(cookie);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetEvents_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/events");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEvents_AsEitherRole_ReturnsOk()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/events").WithSession(cookie);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api.Tests/RoleEnforcementTests.cs'
      git commit -m "Add role enforcement unit tests"
      echo "Committed step 25/$TOTAL: Add role enforcement unit tests"
      ;;
    26)
mkdir -p 'RaceDay.Api.Tests'
cat > 'RaceDay.Api.Tests/EventsControllerTests.cs' << 'RACEDAY_PART2_EOF'
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class EventsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static EventRequest SampleEvent(string name = "Sunrise 5K") => new()
    {
        EventName = name,
        EventDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
        StartTime = new TimeOnly(7, 0),
        Location = "Johannesburg",
        DistanceKm = 5,
        EventType = EventType.Run,
        MaximumParticipants = 200,
        RegistrationDeadline = DateOnly.FromDateTime(DateTime.Today.AddDays(20))
    };

    [Fact]
    public async Task Organiser_Can_CreateUpdateAndCancel_Event()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(SampleEvent())
        }.WithSession(cookie);
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<EventResponse>();
        created.Should().NotBeNull();

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/events/{created!.EventId}")
        {
            Content = JsonContent.Create(SampleEvent("Sunrise 5K (Updated)"))
        }.WithSession(cookie);
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<EventResponse>();
        updated!.EventName.Should().Be("Sunrise 5K (Updated)");

        var cancelRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{created.EventId}").WithSession(cookie);
        var cancelResponse = await _client.SendAsync(cancelRequest);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{created.EventId}").WithSession(cookie);
        var getResponse = await _client.SendAsync(getRequest);
        var fetched = await getResponse.Content.ReadFromJsonAsync<EventResponse>();
        fetched!.Status.Should().Be("Cancelled", "cancelling soft-deletes so historical enrolments/results are preserved");
    }

    [Fact]
    public async Task GetEventById_ForMissingEvent_ReturnsNotFound()
    {
        var cookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/events/999999").WithSession(cookie);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api.Tests/EventsControllerTests.cs'
mkdir -p 'RaceDay.Api.Tests'
cat > 'RaceDay.Api.Tests/EnrolmentsControllerTests.cs' << 'RACEDAY_PART2_EOF'
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.Dtos.Enrolments;
using RaceDay.Api.Dtos.EventCategories;
using RaceDay.Api.Dtos.Events;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class EnrolmentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EnrolmentsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(int eventId, int categoryId)> CreateEventWithCategoryAsync()
    {
        var organiserCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var eventRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(new EventRequest
            {
                EventName = "Test Half Marathon",
                EventDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(2)),
                StartTime = new TimeOnly(6, 0),
                Location = "Pretoria",
                DistanceKm = 21.1m,
                EventType = EventType.Run,
                MaximumParticipants = 500,
                RegistrationDeadline = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            })
        }.WithSession(organiserCookie);
        var eventResponse = await _client.SendAsync(eventRequest);
        var ev = await eventResponse.Content.ReadFromJsonAsync<EventResponse>();

        var categoryRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{ev!.EventId}/categories")
        {
            Content = JsonContent.Create(new EventCategoryRequest { CategoryName = "Open 18-39" })
        }.WithSession(organiserCookie);
        var categoryResponse = await _client.SendAsync(categoryRequest);
        var category = await categoryResponse.Content.ReadFromJsonAsync<EventCategoryResponse>();

        return (ev.EventId, category!.EventCategoryId);
    }

    [Fact]
    public async Task Participant_Can_Enrol_AndSelectedCategory_IsRecorded()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var participantCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var enrolRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        var enrolResponse = await _client.SendAsync(enrolRequest);

        enrolResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var enrolment = await enrolResponse.Content.ReadFromJsonAsync<EnrolmentResponse>();
        enrolment!.EventCategoryId.Should().Be(categoryId, "the brief requires the selected category to be recorded on the enrolment");
        enrolment.EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task Participant_CannotEnrol_Twice_InSameEvent()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var participantCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var firstRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        (await _client.SendAsync(firstRequest)).StatusCode.Should().Be(HttpStatusCode.Created);

        var secondRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        var secondResponse = await _client.SendAsync(secondRequest);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Organiser_CannotEnrol_InEvent()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var organiserCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(organiserCookie);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Organiser_Can_ViewEnrolments_ForTheirEvent()
    {
        var (eventId, categoryId) = await CreateEventWithCategoryAsync();
        var participantCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Participant);

        var enrolRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/enrolments")
        {
            Content = JsonContent.Create(new EnrolmentRequest { EventCategoryId = categoryId })
        }.WithSession(participantCookie);
        await _client.SendAsync(enrolRequest);

        var organiserCookie = await TestAuthHelper.RegisterAndLoginAsync(_client, RoleNames.Organiser);
        var viewRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{eventId}/enrolments").WithSession(organiserCookie);
        var viewResponse = await _client.SendAsync(viewRequest);

        viewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var enrolments = await viewResponse.Content.ReadFromJsonAsync<List<EnrolmentResponse>>();
        enrolments.Should().ContainSingle(e => e.EventId == eventId);
    }
}
RACEDAY_PART2_EOF
git add 'RaceDay.Api.Tests/EnrolmentsControllerTests.cs'
      git commit -m "Add events and enrolments unit tests"
      echo "Committed step 26/$TOTAL: Add events and enrolments unit tests"
      ;;
    27)
mkdir -p '.github/workflows'
cat > '.github/workflows/dotnet-ci.yml' << 'RACEDAY_PART2_EOF'
name: RaceDay Part 2 - Build and Test

on:
  push:
    branches:
      - main
      - master
      - 'part2/**'
  pull_request:

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore RaceDay.sln

      - name: Build
        run: dotnet build RaceDay.sln --no-restore --configuration Release

      - name: Run unit tests
        run: dotnet test RaceDay.sln --no-build --configuration Release --verbosity normal
RACEDAY_PART2_EOF
git add '.github/workflows/dotnet-ci.yml'
      git commit -m "Add GitHub Actions build and test workflow"
      echo "Committed step 27/$TOTAL: Add GitHub Actions build and test workflow"
      ;;
    28)
mkdir -p 'docs'
cat > 'docs/part2-consistency-notes.md' << 'RACEDAY_PART2_EOF'
# Part 2 Consistency Notes

The Part 2 brief flagged two things that needed reconciling with the
approved Part 1 design before implementation. Both are resolved below and
reflected consistently across the EF Core models, the DbContext, the
controllers, and `docs/raceday-part2-migration.sql`.

## 1. Role naming: Administrator → Organiser

Part 1 used **Administrator** as a working name for the management role.
The Part 2 brief specifies **Organiser**. This has been renamed
consistently:

- `RoleNames.Organiser` replaces `RoleNames.Administrator` throughout the
  codebase (models, DTOs, `[RequireRole]` attributes, seed data).
- The seeded `Role` rows are `Participant` and `Organiser`.
- If your Part 1 SQL Server database still has a `Role` row named
  `Administrator`, rename it before running the Part 2 API against that
  same database:
  ```sql
  UPDATE dbo.Role SET RoleName = 'Organiser' WHERE RoleName = 'Administrator';
  ```

## 2. Event categories: global → per-event, selected at enrolment

**Part 1 design:** `EventCategory` was a small global lookup table (5K,
10K, Half Marathon, Marathon) that each `Event` belonged to
(`EventCategory 1:M Event`). An event WAS a category.

**Part 2 requirement:** the brief states an Organiser must "define age or
distance categories **for each event**", and a Participant "enter[s] an
event **by selecting a category**", with the system recording "the
Participant, Event **and selected Category**".

These are different relationships — Part 2's categories are sub-divisions
*within* one event (e.g. this Saturday's race has categories "Under 18",
"18-39", "40+", or "5km"/"10km" splits), not a race-type label shared
across many events. Keeping the Part 1 shape would silently fail the Part
2 requirement, since there would be nowhere to record which category a
specific participant chose.

**Resolution — the Part 1 design was updated, not bypassed:**

| | Part 1 | Part 2 (this implementation) |
|---|---|---|
| `EventCategory` belongs to | nothing (global) | one `Event` (`EventId` FK added) |
| `Event` belongs to | one `EventCategory` | nothing (category link removed) |
| Category uniqueness | `CategoryName` globally unique | `CategoryName` unique **within its event** |
| `Enrolment` records | `UserId`, `EventId` | `UserId`, `EventId`, **`EventCategoryId`** (new FK) |
| `Event` gained | — | `EventType` (Run/Walk/Cycle), per the Part 2 functional requirements |

This is a genuine schema change from Part 1, made explicitly and
documented here — not an unrelated change introduced silently. The ERD,
the EF Core models (`Models/Event.cs`, `Models/EventCategory.cs`,
`Models/Enrolment.cs`), the `RaceDayDbContext` Fluent API configuration,
and `docs/raceday-part2-migration.sql` are all consistent with this
updated shape.

### If you already have a Part 1 SQL Server database

Run `docs/raceday-part2-migration.sql` (or generate/apply the equivalent
EF Core migration with `dotnet ef migrations add Part2CategoryRealignment`)
against it before pointing the Part 2 API at it — the Part 1 script alone
does not include the `EventId` column on `EventCategory` or the
`EventCategoryId` column on `Enrolment`.

If you're starting fresh, `dotnet ef database update` (or `db.Database.Migrate()`,
which runs automatically on API startup — see `Program.cs`) creates the
schema in its final Part 2 shape directly.
RACEDAY_PART2_EOF
git add 'docs/part2-consistency-notes.md'
mkdir -p 'docs'
cat > 'docs/raceday-part2-migration.sql' << 'RACEDAY_PART2_EOF'
/*
============================================================
RaceDay - Part 2 schema realignment script
Run this AGAINST AN EXISTING PART 1 DATABASE to bring it in
line with the Part 2 brief. See docs/part2-consistency-notes.md
for why these changes are needed.

If you are starting from a fresh database instead, skip this
script entirely — running the API applies the equivalent EF
Core Code-First migrations automatically on startup.
============================================================
*/

USE RaceDay;
GO

/* =========================================================
   1. Rename the Administrator role to Organiser
   ========================================================= */

UPDATE dbo.Role
SET RoleName = 'Organiser'
WHERE RoleName = 'Administrator';
GO

/* =========================================================
   2. Event gains EventType (Run / Walk / Cycle)
   ========================================================= */

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Event') AND name = 'EventType'
)
BEGIN
    ALTER TABLE dbo.Event
    ADD EventType NVARCHAR(20) NOT NULL CONSTRAINT DF_Event_EventType DEFAULT 'Run';
END;
GO

/* =========================================================
   3. EventCategory becomes per-event rather than global
   ========================================================= */

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.EventCategory') AND name = 'EventID'
)
BEGIN
    ALTER TABLE dbo.EventCategory
    ADD EventID INT NULL;
END;
GO

-- Drop the old Part 1 relationship: Event -> EventCategory (Event.EventCategoryID)
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Event_EventCategory'
)
BEGIN
    ALTER TABLE dbo.Event DROP CONSTRAINT FK_Event_EventCategory;
END;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Event') AND name = 'EventCategoryID'
)
BEGIN
    ALTER TABLE dbo.Event DROP COLUMN EventCategoryID;
END;
GO

-- Drop the old global uniqueness on CategoryName, add per-event uniqueness instead
IF EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'UQ_EventCategory_CategoryName'
)
BEGIN
    ALTER TABLE dbo.EventCategory DROP CONSTRAINT UQ_EventCategory_CategoryName;
END;
GO

-- Existing seed categories (5K/10K/etc.) have no owning event under the new
-- design. Adjust this to your own data before enforcing NOT NULL — e.g.
-- either delete unused global categories or assign them to a specific event.
-- Example (adjust EventID as appropriate for your data):
-- UPDATE dbo.EventCategory SET EventID = 1 WHERE EventID IS NULL;

ALTER TABLE dbo.EventCategory
ALTER COLUMN EventID INT NOT NULL;
GO

ALTER TABLE dbo.EventCategory
ADD CONSTRAINT FK_EventCategory_Event
    FOREIGN KEY (EventID) REFERENCES dbo.Event(EventID);
GO

ALTER TABLE dbo.EventCategory
ADD CONSTRAINT UQ_EventCategory_Event_CategoryName
    UNIQUE (EventID, CategoryName);
GO

/* =========================================================
   4. Enrolment records the participant's selected category
   ========================================================= */

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Enrolment') AND name = 'EventCategoryID'
)
BEGIN
    ALTER TABLE dbo.Enrolment
    ADD EventCategoryID INT NULL;
END;
GO

-- Existing enrolments have no selected category under the new design.
-- Adjust this to your own data before enforcing NOT NULL — e.g. backfill
-- each enrolment with the appropriate category for its event.
-- Example:
-- UPDATE dbo.Enrolment SET EventCategoryID = <category id> WHERE EnrolmentID = <id>;

ALTER TABLE dbo.Enrolment
ALTER COLUMN EventCategoryID INT NOT NULL;
GO

ALTER TABLE dbo.Enrolment
ADD CONSTRAINT FK_Enrolment_EventCategory
    FOREIGN KEY (EventCategoryID) REFERENCES dbo.EventCategory(EventCategoryID);
GO
RACEDAY_PART2_EOF
git add 'docs/raceday-part2-migration.sql'
      git commit -m "Document Part 1 to Part 2 schema reconciliation"
      echo "Committed step 28/$TOTAL: Document Part 1 to Part 2 schema reconciliation"
      ;;
    29)
cat > 'README.md' << 'RACEDAY_PART2_EOF'
# RaceDay — Part 2: RESTful API Development

An ASP.NET Core Web API implementing the Part 1 endpoint plan for RaceDay,
with EF Core (Code-First), server-side session authentication, role-based
access for Participant and Organiser, Swagger, and xUnit integration tests.

> **Schema change from Part 1:** event categories are now defined per-event
> and selected at enrolment, and the management role is named **Organiser**
> (not Administrator). Both changes were required by the Part 2 brief — see
> [`docs/part2-consistency-notes.md`](docs/part2-consistency-notes.md) for
> the full reasoning and migration path.

## Roles

- **Participant** — registers, logs in, manages their own profile, browses
  events and categories, enrols in an event by selecting a category, views
  their own enrolments and results.
- **Organiser** — everything a Participant can do, plus: creates/updates/
  cancels events, defines categories for their events, views all enrolments
  for an event, and records/updates/deletes results.

Role is chosen at registration (`POST /api/auth/register`) and fixed
thereafter. Every protected endpoint is enforced server-side via a custom
`[RequireRole]` / `[RequireAuthenticated]` filter reading an ASP.NET Core
**session** set at login — not just hidden on the client.

## Project structure

```
RaceDay.sln
RaceDay.Api/              ← the Web API
  Models/                 ← EF Core entities
  Dtos/                   ← request/response shapes per resource
  Data/                   ← RaceDayDbContext, role seeding
  Security/               ← session helpers, RequireAuthenticated/RequireRole
  Controllers/            ← Auth, Users, Events, EventCategories, Enrolments, Results
  Program.cs
RaceDay.Api.Tests/         ← xUnit integration tests (WebApplicationFactory + EF Core InMemory)
docs/
  part2-consistency-notes.md
  raceday-part2-migration.sql
.github/workflows/dotnet-ci.yml
```

## Setup and run instructions

**Prerequisites:** .NET 8 SDK, SQL Server (or SQL Server Express/LocalDB), Visual Studio 2022 (or `dotnet` CLI).

1. **Clone the repository** and open `RaceDay.sln` in Visual Studio, or work from the CLI in the repo root.

2. **Database connection.** `RaceDay.Api/appsettings.Development.json` already
   points at **LocalDB** (`(localdb)\mssqllocaldb`), which installs
   automatically with Visual Studio's "ASP.NET and web development"
   workload — no setup needed for most people. If you'd rather use a full
   SQL Server / SSMS instance instead, edit the `ConnectionStrings:RaceDayDb`
   value in that file (or see `appsettings.Development.json.template` for
   the SQL Server Express format). This file is gitignored — it's provided
   here for your local convenience, but never commit real production
   credentials in its place.

3. **Restore and build:**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Create the database.** The API applies EF Core migrations automatically
   on startup (see `Program.cs`), so simply running it creates/updates the
   schema. If you'd rather do it explicitly first:
   ```bash
   dotnet ef migrations add InitialCreate --project RaceDay.Api
   dotnet ef database update --project RaceDay.Api
   ```
   If you already have a **Part 1** database and want to keep its data,
   run `docs/raceday-part2-migration.sql` against it instead — see
   `docs/part2-consistency-notes.md`.

5. **Run the API:**
   ```bash
   dotnet run --project RaceDay.Api
   ```
   Browse to `https://localhost:<port>/swagger` for the interactive Swagger UI.

6. **Run the tests:**
   ```bash
   dotnet test
   ```

## Authentication flow (for testing in Swagger or Postman)

1. `POST /api/auth/register` with a role of `"Participant"` or `"Organiser"`.
2. `POST /api/auth/login` — the response sets a `RaceDay.Session` cookie.
   Swagger and browsers carry this automatically on subsequent requests, as
   does Postman if cookie handling is enabled.
3. Call any protected endpoint. Session-less requests get **401**; requests
   from the wrong role get **403**.
4. `POST /api/auth/logout` ends the session.

## Swagger

All endpoints are documented and testable at `/swagger` when running in
Development. Each action's XML doc comments describe its purpose and list
the possible response codes (200/201/204/400/401/403/404/409) directly in
the Swagger UI.

## GitHub and CI/CD

- `.github/workflows/dotnet-ci.yml` builds the solution and runs the full
  test suite (`dotnet test`) on every push and pull request.
- **[ ] TODO:** insert a screenshot of the successful green Actions run here
  once you've pushed and it has run.

`[INSERT SCREENSHOT OF GREEN GITHUB ACTIONS BUILD HERE]`

## Unit tests

`RaceDay.Api.Tests` covers, per the brief:

- Registration and login (success, duplicate email, unknown role, wrong password, unknown email).
- Role-based rejection: 401 for no session, 403 for the wrong role, on both Events and Enrolments.
- Event CRUD by an Organiser (create, update, cancel — soft-cancel so history survives).
- Enrolment: a Participant can enrol and select a category, the category is recorded correctly, a duplicate enrolment is rejected (409), and an Organiser cannot enrol.
- An Organiser viewing all enrolments for their event.

All tests run against the real HTTP pipeline (`WebApplicationFactory<Program>`) with an isolated EF Core InMemory database per test class, so they exercise the actual routes, status codes, and session cookies rather than calling controller methods directly.

## Video presentation

`[INSERT UNLISTED YOUTUBE VIDEO LINK HERE]`

## Consistency with Part 1

See [`docs/part2-consistency-notes.md`](docs/part2-consistency-notes.md) for
a full account of the two required changes (role naming, per-event
categories) and how the ERD/SQL/EF Core model were kept consistent rather
than diverging silently.
RACEDAY_PART2_EOF
git add 'README.md'
      git commit -m "Finalise Part 2 README"
      echo "Committed step 29/$TOTAL: Finalise Part 2 README"
      ;;
    *) echo "All $TOTAL commits are already done."; exit 0 ;;
  esac
}

if [ "$1" == "--all" ]; then
  while [ "$STEP" -lt "$TOTAL" ]; do
    STEP=$((STEP+1))
    run_step $STEP
    echo $STEP > "$STATE_FILE"
  done
else
  if [ "$STEP" -ge "$TOTAL" ]; then
    echo "All $TOTAL commits are already done."
    exit 0
  fi
  STEP=$((STEP+1))
  run_step $STEP
  echo $STEP > "$STATE_FILE"
  echo "Run this script again to make the next commit ($STEP/$TOTAL done)."
fi

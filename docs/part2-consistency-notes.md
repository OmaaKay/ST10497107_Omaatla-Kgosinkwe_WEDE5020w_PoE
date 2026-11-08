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

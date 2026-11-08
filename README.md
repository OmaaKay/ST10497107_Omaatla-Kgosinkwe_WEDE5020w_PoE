# RaceDay: Part 3 (MVC web front end, Azure Blob Storage, Docker)

RaceDay is a race and event management platform. **Participants** browse upcoming events, enrol in a category and track their personal results; **Organisers** create events, manage categories and enrolments, and publish finish times and positions.

| Part | What | Folder |
|------|------|--------|
| 2 | RESTful ASP.NET Core Web API (EF Core, SQL Server, session auth, roles, Swagger, tests) + **Azure Blob Storage added in Part 3** | `RaceDay.Api/`, `RaceDay.Api.Tests/` |
| 3 | ASP.NET Core **MVC** web app that consumes the API, session-based role navigation, Docker | `RaceDay.Mvc/`, `Dockerfile` |

> **CI:** GitHub Actions workflow `.github/workflows/dotnet-ci.yml` (see the repository's Actions tab for the green build).

## Architecture

```
Browser ──> RaceDay.Mvc (MVC, session) ──HTTP/JSON──> RaceDay.Api ──> SQL Server
                                                         │
                                                         └──> Azure Blob Storage (images)
```

* The MVC project has **no database access and no business logic**. Every data operation goes through `Services/ApiClient.cs`.
* **Session auth:** on login the MVC app calls `POST /api/auth/login`, captures the API's session cookie, and stores it (plus user id, role and name) in the MVC session. The cookie is replayed on every API call, so both sides agree who is signed in. Logout clears both sessions.
* **Blob Storage:** the MVC app uploads a file to the API (`POST /api/events/{id}/banner`, `POST /api/users/me/picture`); only the API talks to Azure. The returned blob URL is saved in the database and rendered by MVC.

```
RaceDay.sln
├── RaceDay.Api/            Web API (Models, Dtos, Data, Security, Services/BlobStorage, Controllers)
├── RaceDay.Api.Tests/      xUnit integration tests (Part 2)
├── RaceDay.Mvc/            MVC front end
│   ├── Controllers/        Account, Home, Events, MyEvents, MyResults, Profile, Organiser*
│   ├── Services/           ApiClient (HttpClient wrapper), ApiResult
│   ├── Infrastructure/     SessionAuthorize filter, session helpers, base controllers
│   ├── Models/             Api DTOs + view models
│   └── Views/ + wwwroot/   Razor views, RaceDay theme (css/js)
├── Dockerfile              MVC image          Dockerfile.api  API image
├── docker-compose.yml      SQL Server + API + MVC in one command
├── .github/workflows/dotnet-ci.yml
└── docs/                   migration SQL, video script, submission checklist
```

## Features

**Everyone:** register (choose Participant or Organiser), log in, log out, update profile and upload a profile picture.
**Participant:** home page of upcoming events with filters (search, type, date range) · event detail page with categories · enrol with a category (recorded immediately) · *My Events* with colour-coded status and cancel · *My Results* (event, date, category, finish time, position such as "47th out of 312 finishers").
**Organiser:** dashboard (events created, total enrolments, upcoming dates) · create/edit/delete events with banner upload · category management per event · enrolments page (confirm / cancel) · results entry (position and time, published instantly).
**Status colours:** Pending = amber, Confirmed = green, Cancelled = red, Completed = blue.

## Run it

### Prerequisites
.NET 8 SDK, SQL Server / LocalDB, Docker Desktop, an Azure Storage account.

### 1. Azure Blob Storage (one-off)
1. Azure Portal → **Storage accounts → Create** (Standard, LRS).
2. In the account: **Settings → Configuration → Allow Blob anonymous access = Enabled** (images are served by public URL).
3. **Security + networking → Access keys → Connection string** → copy it.
4. Give it to the API (never commit it):
   ```bash
   cd RaceDay.Api
   dotnet user-secrets set "AzureBlob:ConnectionString" "<your connection string>"
   ```
   The API creates the `raceday-images` container automatically on the first upload.

### 2. Database
`RaceDay.Api/appsettings.Development.json` points at LocalDB. On start the API creates the schema (or applies migrations if you have a `Migrations` folder). Upgrading a Part 2 database? Run `docs/raceday-part3-migration.sql` first.

### 3. Run locally (two terminals)
```bash
dotnet run --project RaceDay.Api --launch-profile http     # API  -> http://localhost:5100/swagger
dotnet run --project RaceDay.Mvc                           # MVC  -> http://localhost:5200
```
The MVC app finds the API through `ApiSettings:BaseUrl` (default `http://localhost:5100`).

### 4. Run the MVC app in Docker (single command)
Start the API as above (on Linux add `--urls http://0.0.0.0:5100`), then:

```bash
docker build -t raceday-mvc . && docker run --rm -p 8080:8080 --add-host=host.docker.internal:host-gateway -e ApiSettings__BaseUrl=http://host.docker.internal:5100 raceday-mvc
```
Open **http://localhost:8080**. The container behaves exactly like the local run because the only difference is the API address, supplied by environment variable.

**Whole stack in containers** (SQL Server + API + MVC):
```bash
export AZURE_STORAGE_CONNECTION_STRING="<connection string>"   # PowerShell: $env:AZURE_STORAGE_CONNECTION_STRING="..."
docker compose up --build
```
MVC → http://localhost:8080, API/Swagger → http://localhost:5100/swagger.

### Quick demo flow
1. Register an **Organiser** → create an event with a banner → add categories.
2. Log out, register a **Participant** → open the event → enrol → see *Pending* in My Events.
3. Log in as the Organiser → *Enrolments* → Confirm → *Results* → enter position and time.
4. Log in as the Participant → *My Results*.

## CI/CD
`.github/workflows/dotnet-ci.yml` validates the repository structure, restores and builds the API **and** MVC projects, runs the tests, then builds the MVC Docker image.

## Notes
* Deleting an event is a soft delete (status *Cancelled*) so historical enrolments and results are never lost.
* Invalid input never shows raw errors: forms re-display with field messages, and unexpected failures show a friendly page.


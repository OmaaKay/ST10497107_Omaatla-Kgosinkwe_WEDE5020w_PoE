#!/usr/bin/env bash
# RaceDay Part 3 - commit helper.
# Run from the ROOT of your git repository (after `git checkout -b part3`).
# Each run stages the next group of Part 3 files and makes ONE commit, then stops,
# so you can build/test and push between steps:
#     ./docs/commit-part3.sh && git push -u origin part3
# Tip: do 3-5 steps per sitting, run `dotnet build` in between, and read each group as you commit it.
set -e
STATE=.part3_commit_step
[ -f "$STATE" ] || echo 0 > "$STATE"
STEP=$(cat "$STATE")

MSGS=(
"API: add banner, organiser and profile picture fields to models and DTOs"
"API: add Azure Blob Storage service and package reference"
"API: add event banner upload endpoint and organiser filter"
"API: add profile picture upload endpoint"
"API: enrolment Pending/Confirmed statuses and organiser status endpoint"
"API: add personal results endpoint with total finishers"
"API: wire blob service, schema creation and launch settings"
"MVC: create project skeleton and configuration"
"MVC: add ApiClient with session cookie forwarding"
"MVC: add session helpers, role filter and base controllers"
"MVC: add API models, view models and display helpers"
"MVC: add layout, RaceDay theme and shared partials"
"MVC: add register, login and logout"
"MVC: add landing page and participant home with filters"
"MVC: add event details and enrolment"
"MVC: add My Events with cancel"
"MVC: add My Results page"
"MVC: add profile page with picture upload"
"MVC: add organiser dashboard"
"MVC: add organiser event management with banner upload"
"MVC: add category management"
"MVC: add organiser enrolments page"
"MVC: add results entry page"
"Docker: add Dockerfiles, dockerignore and compose"
"CI: build MVC project and Docker image, add solution entry"
"Docs: README, migration script, video script and checklist"
)
PATHS=(
"RaceDay.Api/Models RaceDay.Api/Dtos"
"RaceDay.Api/Services RaceDay.Api/RaceDay.Api.csproj"
"RaceDay.Api/Controllers/EventsController.cs"
"RaceDay.Api/Controllers/UsersController.cs"
"RaceDay.Api/Controllers/EnrolmentsController.cs"
"RaceDay.Api/Controllers/ResultsController.cs"
"RaceDay.Api/Program.cs RaceDay.Api/appsettings.json RaceDay.Api/Properties"
"RaceDay.Mvc/RaceDay.Mvc.csproj RaceDay.Mvc/Program.cs RaceDay.Mvc/appsettings.json RaceDay.Mvc/Properties"
"RaceDay.Mvc/Services"
"RaceDay.Mvc/Infrastructure/UserSession.cs RaceDay.Mvc/Infrastructure/SessionAuthorizeAttribute.cs RaceDay.Mvc/Infrastructure/AppController.cs RaceDay.Mvc/Infrastructure/OrganiserBase.cs RaceDay.Mvc/Infrastructure/ImageRules.cs"
"RaceDay.Mvc/Models RaceDay.Mvc/Infrastructure/Display.cs"
"RaceDay.Mvc/Views/_ViewImports.cshtml RaceDay.Mvc/Views/_ViewStart.cshtml RaceDay.Mvc/Views/Shared RaceDay.Mvc/wwwroot"
"RaceDay.Mvc/Controllers/AccountController.cs RaceDay.Mvc/Views/Account"
"RaceDay.Mvc/Controllers/HomeController.cs RaceDay.Mvc/Views/Home"
"RaceDay.Mvc/Controllers/EventsController.cs RaceDay.Mvc/Views/Events"
"RaceDay.Mvc/Controllers/MyEventsController.cs RaceDay.Mvc/Views/MyEvents"
"RaceDay.Mvc/Controllers/MyResultsController.cs RaceDay.Mvc/Views/MyResults"
"RaceDay.Mvc/Controllers/ProfileController.cs RaceDay.Mvc/Views/Profile"
"RaceDay.Mvc/Controllers/OrganiserController.cs RaceDay.Mvc/Views/Organiser"
"RaceDay.Mvc/Controllers/OrganiserEventsController.cs RaceDay.Mvc/Views/OrganiserEvents"
"RaceDay.Mvc/Controllers/OrganiserCategoriesController.cs RaceDay.Mvc/Views/OrganiserCategories"
"RaceDay.Mvc/Controllers/OrganiserEnrolmentsController.cs RaceDay.Mvc/Views/OrganiserEnrolments"
"RaceDay.Mvc/Controllers/OrganiserResultsController.cs RaceDay.Mvc/Views/OrganiserResults"
"Dockerfile Dockerfile.api .dockerignore docker-compose.yml"
".github RaceDay.sln .gitignore"
"README.md docs"
)

TOTAL=${#MSGS[@]}
if [ "$STEP" -ge "$TOTAL" ]; then echo "All $TOTAL commits done. Now: git push"; exit 0; fi

git add -A -- ${PATHS[$STEP]}
if git diff --cached --quiet; then
  echo "Nothing new to commit in group $((STEP+1)); skipping."
else
  git commit -q -m "${MSGS[$STEP]}"
  echo "Committed $((STEP+1))/$TOTAL: ${MSGS[$STEP]}"
fi
echo $((STEP+1)) > "$STATE"

# YouTube video script (target 8–10 minutes, unlisted)

Rubric wants: full running app, MVC structure, Blob Storage, Docker, **clear code explanation**.

1. **Intro (0:30)**: what RaceDay is, two roles, Part 2 API + Part 3 MVC.
2. **Architecture (1:30)**: show the solution tree. MVC has no DbContext; everything through `ApiClient`. Explain cookie capture on login and the `SessionAuthorize` filter.
3. **Auth demo (1:00)**: register, bad login message, login, different menus per role, open a protected URL while logged out (redirected), log out.
4. **Organiser demo (2:30)**: dashboard, create event with banner, categories, (after a participant enrols) confirm enrolment, enter results.
5. **Participant demo (2:00)**: filters, event detail, enrol, My Events (amber Pending → green Confirmed), My Results ("47th out of N finishers"), profile picture upload.
6. **Blob Storage (1:00)**: show `BlobStorageService`, the upload endpoint, the container in the Azure portal, the URL stored in the database and displayed in MVC.
7. **Docker (1:00)**: show the Dockerfile (multi-stage, non-root), run the README command, show the app on :8080.
8. **CI (0:30)**: green GitHub Actions run, 20+ commits.

# Suggested commit plan (20+ meaningful commits)

Commit as you genuinely work through and test each step, so the history reflects real progress. Suggested order:

1. Add Part 3 branch and update README skeleton
2. API: add Event.BannerUrl, Event.OrganiserId, User.ProfilePictureUrl
3. API: Azure Blob service (interface, options, implementation)
4. API: banner upload endpoint
5. API: profile picture endpoint
6. API: enrolment Pending/Confirmed status + organiser status endpoint
7. API: GET /api/results/me with total finishers
8. API: Program.cs wiring, launch settings, schema creation
9. MVC: project skeleton, Program.cs, appsettings
10. MVC: ApiClient and ApiResult
11. MVC: session helpers and SessionAuthorize filter
12. MVC: layout, site.css theme, shared partials
13. MVC: register/login/logout
14. MVC: landing page and participant home with filters
15. MVC: event details and enrol
16. MVC: My Events and cancel
17. MVC: My Results
18. MVC: profile page and picture upload
19. MVC: organiser dashboard
20. MVC: organiser events CRUD with banner
21. MVC: category management
22. MVC: enrolments page
23. MVC: results entry
24. Docker: Dockerfile, .dockerignore, compose
25. CI: build MVC project and Docker image
26. Docs: README, video link, CI screenshot

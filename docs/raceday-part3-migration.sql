-- RaceDay Part 3 schema changes, for an EXISTING Part 2 database.
-- A brand-new database needs nothing: the API creates the full schema on first start.
-- (If you use EF migrations instead:  dotnet ef migrations add Part3Changes --project RaceDay.Api)

IF COL_LENGTH('Events', 'BannerUrl') IS NULL
    ALTER TABLE Events ADD BannerUrl NVARCHAR(500) NULL;

IF COL_LENGTH('Events', 'OrganiserId') IS NULL
    ALTER TABLE Events ADD OrganiserId INT NULL;

IF COL_LENGTH('Users', 'ProfilePictureUrl') IS NULL
    ALTER TABLE Users ADD ProfilePictureUrl NVARCHAR(500) NULL;

-- Enrolment statuses are now Pending / Confirmed / Cancelled / Completed.
UPDATE Enrolments SET Status = 'Confirmed' WHERE Status = 'Active';

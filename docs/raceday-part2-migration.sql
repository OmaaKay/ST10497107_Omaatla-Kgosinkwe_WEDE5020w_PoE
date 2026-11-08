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

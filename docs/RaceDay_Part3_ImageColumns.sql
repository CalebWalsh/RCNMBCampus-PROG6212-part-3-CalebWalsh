-- Part 3: adds the two image URL columns that the Azure Blob uploads save into.
-- Use this only if you cannot apply them with an EF Core migration.
-- Change the database name if yours is not RaceDayDB.
USE RaceDayDB;
GO

IF COL_LENGTH('dbo.[Event]', 'BannerImageUrl') IS NULL
    ALTER TABLE dbo.[Event] ADD BannerImageUrl nvarchar(500) NULL;
GO

IF COL_LENGTH('dbo.[User]', 'ProfileImageUrl') IS NULL
    ALTER TABLE dbo.[User] ADD ProfileImageUrl nvarchar(500) NULL;
GO

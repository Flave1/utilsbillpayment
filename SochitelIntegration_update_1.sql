-- Commands to be executed for the Sochitel Integration update.

-- PlatformApiLogs
-- PlatformApis
-- PlatformApiConnections
-- PlatformPacParam
-- PlatformTransactions


-- Add the 'PlatformType' column to differentiate products
ALTER TABLE dbo.Platform 
ADD [PlatformType] INT NOT NULL DEFAULT -1;

ALTER TABLE dbo.Platform
ADD [PlatformApiConnId] INT NULL
CONSTRAINT [FK_Platform_PlatformApiConnections] FOREIGN KEY ([PlatformApiConnId]) REFERENCES [dbo].[PlatformApiConnections] ([Id]);

ALTER TABLE dbo.Platform
ADD [PlatformApiConnBackupId] INT NULL
CONSTRAINT [FK_Platform_PlatformApiConnections_Backup] FOREIGN KEY ([PlatformApiConnBackupId]) REFERENCES [dbo].[PlatformApiConnections] ([Id]);



CREATE TABLE [dbo].[Currencies] (
    [Id]        NVARCHAR (3)    NOT NULL,
    [Name]      NVARCHAR (75)   NOT NULL,
    CONSTRAINT [PK_Currencies] PRIMARY KEY CLUSTERED ([Id] ASC)
);

INSERT INTO dbo.Currencies([Id], [Name])
VALUES('SLE', 'Sierra Leone LEONE (SLE)');



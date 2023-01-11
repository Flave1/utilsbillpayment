-- Commands to be executed for the Sochitel Integration update.

-- PlatformApiLogs
CREATE TABLE [dbo].[PlatformApiLogs] (
    [Id]            BIGINT         IDENTITY (1, 1) NOT NULL,
    [TransactionId] BIGINT         NOT NULL,
    [ApiLog]        NVARCHAR (MAX) NOT NULL,
    [LogType]       INT            NOT NULL,
    [LogDate]       DATETIME       NOT NULL,
    CONSTRAINT [PK_PlatformApiLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformApiLogs_1]
    ON [dbo].[PlatformApiLogs]([TransactionId] ASC);


-- PlatformApis
CREATE TABLE [dbo].[PlatformApis] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (75)   NOT NULL,
    [ApiType]   INT             NOT NULL,
    [Balance]   DECIMAL (16, 2) CONSTRAINT [DEFAULT_PlatformApis_Balance] DEFAULT ((0.00)) NOT NULL,
    [Status]    INT             NOT NULL,
    [CreatedAt] DATETIME        NULL,
    [UpdatedAt] DATETIME        NULL,
    [Currency]  NVARCHAR (3)    NOT NULL,
    [Config]    NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_PlatformApis] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlatformApis_Currencies] FOREIGN KEY ([Currency]) REFERENCES [dbo].[Currencies] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformApis_1]
    ON [dbo].[PlatformApis]([ApiType] ASC);

-- PlatformApiConnections
CREATE TABLE [dbo].[PlatformApiConnections] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [PlatformId]    INT            NOT NULL,
    [PlatformApiId] INT            NULL,
    [Name]          NVARCHAR (100) NOT NULL,
    [Status]        INT            NOT NULL,
    [CreatedAt]     DATETIME       NOT NULL,
    [UpdatedAt]     DATETIME       NOT NULL,
    CONSTRAINT [PK_PlatformApiConnections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlatformApiConnections_Platform] FOREIGN KEY ([PlatformId]) REFERENCES [dbo].[Platform] ([PlatformId]),
    CONSTRAINT [FK_PlatformApiConnections_PlatformApis] FOREIGN KEY ([PlatformApiId]) REFERENCES [dbo].[PlatformApis] ([Id])
);

-- PlatformPacParams
CREATE TABLE [dbo].[PlatformPacParams] (
    [Id]                      BIGINT          IDENTITY (1, 1) NOT NULL,
    [PlatformId]              INT             NOT NULL,
    [PlatformApiConnectionId] INT             NOT NULL,
    [Config]                  NVARCHAR (4000) NULL,
    [CreatedAt]               DATETIME        NOT NULL,
    [UpdatedAt]               DATETIME        NOT NULL,
    CONSTRAINT [PK_PlatformPacParams] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlatformPacParams_Platform] FOREIGN KEY ([PlatformId]) REFERENCES [dbo].[Platform] ([PlatformId]),
    CONSTRAINT [FK_PlatformPacParams_PlatformApiConnections] FOREIGN KEY ([PlatformApiConnectionId]) REFERENCES [dbo].[PlatformApiConnections] ([Id])
);


-- PlatformTransactions
CREATE TABLE [dbo].[PlatformTransactions] (
    [Id]                BIGINT          IDENTITY (1, 1) NOT NULL,
    [UserId]            BIGINT          NOT NULL,
    [PlatformId]        INT             NOT NULL,
    [Amount]            DECIMAL (16, 2) CONSTRAINT [DEFAULT_PlatformTransactions_Amount] DEFAULT ((0.00)) NOT NULL,
    [AmountPlatform]    DECIMAL (16, 2) CONSTRAINT [DEFAULT_PlatformTransactions_AmountPlatform] DEFAULT ((0.00)) NOT NULL,
    [Status]            INT             NOT NULL,
    [Beneficiary]       NVARCHAR (150)  NULL,
    [UserReference]     NVARCHAR (32)   NULL,
    [ApiConnectionId]   INT             NULL,
    [Currency]          NVARCHAR (3)    NOT NULL,
    [CreatedAt]         DATETIME        CONSTRAINT [DEFAULT_PlatformTransactions_CreatedAt] DEFAULT (getdate()) NOT NULL,
    [UpdatedAt]         DATETIME        NOT NULL,
    [PosId]             BIGINT          NULL,
    [OperatorReference] NVARCHAR (150)  NULL,
    [PinSerial]         NVARCHAR (150)  NULL,
    [PinNumber]         NVARCHAR (1000) NULL,
    [PinInstructions]   NVARCHAR (MAX)  NULL,
    [ApiTransactionId]  NVARCHAR (50)   NULL,
    [LastPendingCheck]  BIGINT          CONSTRAINT [DEFAULT_PlatformTransactions_LastPendingCheck] DEFAULT ((0)) NOT NULL,
    [TransactionDetailId]  BIGINT      NOT NULL,
    CONSTRAINT [PK_PlatformTransactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlatformTransactions_Currencies] FOREIGN KEY ([Currency]) REFERENCES [dbo].[Currencies] ([Id]),
    CONSTRAINT [FK_PlatformTransactions_Platform] FOREIGN KEY ([PlatformId]) REFERENCES [dbo].[Platform] ([PlatformId]),
    CONSTRAINT [FK_PlatformTransactions_PlatformApiConnections] FOREIGN KEY ([ApiConnectionId]) REFERENCES [dbo].[PlatformApiConnections] ([Id]),
    CONSTRAINT [FK_PlatformTransactions_POS] FOREIGN KEY ([PosId]) REFERENCES [dbo].[POS] ([POSId]),
    CONSTRAINT [FK_PlatformTransactions_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_6]
    ON [dbo].[PlatformTransactions]([UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_3]
    ON [dbo].[PlatformTransactions]([UserReference] ASC);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_8]
    ON [dbo].[PlatformTransactions]([ApiConnectionId] ASC);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_1]
    ON [dbo].[PlatformTransactions]([Status] DESC);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_5]
    ON [dbo].[PlatformTransactions]([OperatorReference] ASC);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_2]
    ON [dbo].[PlatformTransactions]([Beneficiary] ASC);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_7]
    ON [dbo].[PlatformTransactions]([PlatformId] ASC);


GO
CREATE NONCLUSTERED INDEX [Index_PlatformTransactions_4]
    ON [dbo].[PlatformTransactions]([ApiTransactionId] ASC);


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



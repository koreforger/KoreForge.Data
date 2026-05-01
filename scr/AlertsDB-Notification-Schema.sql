-- =============================================================================
-- AlertsDB — Notification Schema
-- =============================================================================
-- This script creates the Notification schema used by the AlertsDB database.
-- Lookup tables use FK relationships — no C# enums.
-- Run this against an empty database or one where the schema does not yet exist.
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Notification')
    EXEC('CREATE SCHEMA Notification');
GO

-- ---------------------------------------------------------------------------
-- Lookup tables
-- ---------------------------------------------------------------------------

CREATE TABLE Notification.Channel (
    ChannelId   INT           IDENTITY(1,1)  NOT NULL,
    Name        NVARCHAR(50)  NOT NULL,
    CONSTRAINT PK_Channel PRIMARY KEY (ChannelId),
    CONSTRAINT UQ_Channel_Name UNIQUE (Name)
);
GO

CREATE TABLE Notification.Priority (
    PriorityId  INT           IDENTITY(1,1)  NOT NULL,
    Name        NVARCHAR(50)  NOT NULL,
    CONSTRAINT PK_Priority PRIMARY KEY (PriorityId),
    CONSTRAINT UQ_Priority_Name UNIQUE (Name)
);
GO

CREATE TABLE Notification.OutboxStatus (
    OutboxStatusId  INT           IDENTITY(1,1)  NOT NULL,
    Name            NVARCHAR(50)  NOT NULL,
    CONSTRAINT PK_OutboxStatus PRIMARY KEY (OutboxStatusId),
    CONSTRAINT UQ_OutboxStatus_Name UNIQUE (Name)
);
GO

CREATE TABLE Notification.SendOutcome (
    SendOutcomeId   INT           IDENTITY(1,1)  NOT NULL,
    Name            NVARCHAR(50)  NOT NULL,
    CONSTRAINT PK_SendOutcome PRIMARY KEY (SendOutcomeId),
    CONSTRAINT UQ_SendOutcome_Name UNIQUE (Name)
);
GO

-- ---------------------------------------------------------------------------
-- Core tables
-- ---------------------------------------------------------------------------

CREATE TABLE Notification.NotificationOutbox (
    NotificationOutboxId  BIGINT            IDENTITY(1,1)  NOT NULL,
    CorrelationId         UNIQUEIDENTIFIER  NOT NULL,
    ChannelId             INT               NOT NULL,
    PriorityId            INT               NOT NULL,
    OutboxStatusId        INT               NOT NULL,
    SendOutcomeId         INT               NULL,
    Recipient             NVARCHAR(500)     NOT NULL,
    Subject               NVARCHAR(500)     NULL,
    Body                  NVARCHAR(MAX)     NOT NULL,
    RetryCount            INT               NOT NULL  DEFAULT 0,
    NextRetryAt           DATETIMEOFFSET    NULL,
    CreatedAt             DATETIMEOFFSET    NOT NULL  DEFAULT SYSDATETIMEOFFSET(),
    UpdatedAt             DATETIMEOFFSET    NOT NULL  DEFAULT SYSDATETIMEOFFSET(),
    SentAt                DATETIMEOFFSET    NULL,
    CONSTRAINT PK_NotificationOutbox PRIMARY KEY (NotificationOutboxId),
    CONSTRAINT FK_NotificationOutbox_Channel      FOREIGN KEY (ChannelId)      REFERENCES Notification.Channel(ChannelId),
    CONSTRAINT FK_NotificationOutbox_Priority     FOREIGN KEY (PriorityId)     REFERENCES Notification.Priority(PriorityId),
    CONSTRAINT FK_NotificationOutbox_OutboxStatus FOREIGN KEY (OutboxStatusId) REFERENCES Notification.OutboxStatus(OutboxStatusId),
    CONSTRAINT FK_NotificationOutbox_SendOutcome  FOREIGN KEY (SendOutcomeId)  REFERENCES Notification.SendOutcome(SendOutcomeId)
);
GO

CREATE TABLE Notification.EmailPayload (
    EmailPayloadId        BIGINT        IDENTITY(1,1)  NOT NULL,
    NotificationOutboxId  BIGINT        NOT NULL,
    FromAddress           NVARCHAR(500) NOT NULL,
    CcRecipients          NVARCHAR(MAX) NULL,
    BccRecipients         NVARCHAR(MAX) NULL,
    IsHtml                BIT           NOT NULL  DEFAULT 1,
    CONSTRAINT PK_EmailPayload PRIMARY KEY (EmailPayloadId),
    CONSTRAINT FK_EmailPayload_NotificationOutbox FOREIGN KEY (NotificationOutboxId) REFERENCES Notification.NotificationOutbox(NotificationOutboxId)
);
GO

CREATE TABLE Notification.SmsPayload (
    SmsPayloadId          BIGINT        IDENTITY(1,1)  NOT NULL,
    NotificationOutboxId  BIGINT        NOT NULL,
    FromNumber            NVARCHAR(20)  NULL,
    ProviderMessageId     NVARCHAR(200) NULL,
    CONSTRAINT PK_SmsPayload PRIMARY KEY (SmsPayloadId),
    CONSTRAINT FK_SmsPayload_NotificationOutbox FOREIGN KEY (NotificationOutboxId) REFERENCES Notification.NotificationOutbox(NotificationOutboxId)
);
GO

-- ---------------------------------------------------------------------------
-- Indexes
-- ---------------------------------------------------------------------------

CREATE INDEX IX_NotificationOutbox_CorrelationId
    ON Notification.NotificationOutbox (CorrelationId);

CREATE INDEX IX_NotificationOutbox_OutboxStatusId_CreatedAt
    ON Notification.NotificationOutbox (OutboxStatusId, CreatedAt);

CREATE INDEX IX_EmailPayload_NotificationOutboxId
    ON Notification.EmailPayload (NotificationOutboxId);

CREATE INDEX IX_SmsPayload_NotificationOutboxId
    ON Notification.SmsPayload (NotificationOutboxId);
GO

-- ---------------------------------------------------------------------------
-- Seed data — lookup tables
-- ---------------------------------------------------------------------------

INSERT INTO Notification.Channel (Name) VALUES
    ('Email'), ('SMS'), ('Push'), ('InApp');

INSERT INTO Notification.Priority (Name) VALUES
    ('Low'), ('Normal'), ('High'), ('Critical');

INSERT INTO Notification.OutboxStatus (Name) VALUES
    ('Pending'), ('Processing'), ('Sent'), ('Failed'), ('Cancelled');

INSERT INTO Notification.SendOutcome (Name) VALUES
    ('Success'), ('HardBounce'), ('SoftBounce'), ('Rejected'), ('Timeout'), ('ProviderError');
GO

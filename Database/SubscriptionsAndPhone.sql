-- =====================================================================
-- StreamZoneDB — Migrație: Telefon + Abonamente + Tracking vizionări
-- Rulează în SSMS pe baza de date StreamZoneDB
-- =====================================================================

USE StreamZoneDB;
GO

-- ─── Coloane noi în Users ────────────────────────────────────────────
IF COL_LENGTH('dbo.Users', 'PhoneNumber') IS NULL
    ALTER TABLE dbo.Users ADD PhoneNumber NVARCHAR(20) NULL;
GO

IF COL_LENGTH('dbo.Users', 'SubscriptionType') IS NULL
    ALTER TABLE dbo.Users ADD SubscriptionType NVARCHAR(20) NOT NULL DEFAULT 'Free';
GO

IF COL_LENGTH('dbo.Users', 'SubscriptionExpiresAt') IS NULL
    ALTER TABLE dbo.Users ADD SubscriptionExpiresAt DATETIME2 NULL;
GO

-- ─── Tabel WatchEvents (pentru limitele zilnice) ─────────────────────
IF OBJECT_ID('dbo.WatchEvents', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WatchEvents (
        WatchEventId  INT IDENTITY(1,1) PRIMARY KEY,
        UserId        INT            NOT NULL,
        ContentTitle  NVARCHAR(200)  NOT NULL,
        ContentType   NVARCHAR(20)   NOT NULL,    -- 'Movie' | 'Series' | 'Documentary'
        Quality       NVARCHAR(10)   NULL,        -- '720p' | '1080p' | '4K'
        WatchedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_WatchEvents_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        CONSTRAINT CK_WatchEvents_Type CHECK (ContentType IN ('Movie','Series','Documentary'))
    );
    CREATE INDEX IX_WatchEvents_User_Date ON dbo.WatchEvents(UserId, WatchedAt);
END
GO

-- ─── Verificare ──────────────────────────────────────────────────────
SELECT
    COL_LENGTH('dbo.Users','PhoneNumber')          AS HasPhoneNumber,
    COL_LENGTH('dbo.Users','SubscriptionType')     AS HasSubscriptionType,
    COL_LENGTH('dbo.Users','SubscriptionExpiresAt') AS HasSubscriptionExpiresAt,
    OBJECT_ID('dbo.WatchEvents','U')               AS WatchEventsTableId;
GO

PRINT '✓ Migrație aplicată: PhoneNumber, SubscriptionType, SubscriptionExpiresAt + WatchEvents';
GO

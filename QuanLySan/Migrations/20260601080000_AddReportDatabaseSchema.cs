using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuanLySan.Data;

#nullable disable

namespace QuanLySan.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260601080000_AddReportDatabaseSchema")]
    public partial class AddReportDatabaseSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Roles
                    (
                        RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
                        RoleName NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE
                    );
                END;

                IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Customer')
                    INSERT INTO dbo.Roles (RoleName) VALUES (N'Customer');
                IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'FieldOwner')
                    INSERT INTO dbo.Roles (RoleName) VALUES (N'FieldOwner');
                IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Admin')
                    INSERT INTO dbo.Roles (RoleName) VALUES (N'Admin');

                IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Users
                    (
                        UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
                        FullName NVARCHAR(200) NOT NULL,
                        Email NVARCHAR(256) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
                        PasswordHash NVARCHAR(MAX) NOT NULL,
                        PhoneNumber NVARCHAR(20) NULL,
                        RoleId INT NOT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
                        IsEmailVerified BIT NOT NULL CONSTRAINT DF_Users_IsEmailVerified DEFAULT (0),
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()),
                        CONSTRAINT FK_Users_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
                    );
                    CREATE INDEX IX_Users_RoleId ON dbo.Users(RoleId);
                END;

                INSERT INTO dbo.Users (FullName, Email, PasswordHash, PhoneNumber, RoleId, IsActive, IsEmailVerified, CreatedAt)
                SELECT
                    COALESCE(NULLIF(au.UserName, N''), au.Email, au.Id),
                    COALESCE(NULLIF(au.Email, N''), au.Id + N'@local'),
                    COALESCE(au.PasswordHash, N''),
                    au.PhoneNumber,
                    CASE
                        WHEN EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur JOIN dbo.AspNetRoles ar ON ar.Id = ur.RoleId WHERE ur.UserId = au.Id AND ar.Name = N'Admin')
                            THEN (SELECT TOP 1 RoleId FROM dbo.Roles WHERE RoleName = N'Admin')
                        WHEN EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur JOIN dbo.AspNetRoles ar ON ar.Id = ur.RoleId WHERE ur.UserId = au.Id AND ar.Name IN (N'Owner', N'ChuSan'))
                            THEN (SELECT TOP 1 RoleId FROM dbo.Roles WHERE RoleName = N'FieldOwner')
                        ELSE (SELECT TOP 1 RoleId FROM dbo.Roles WHERE RoleName = N'Customer')
                    END,
                    1,
                    au.EmailConfirmed,
                    GETDATE()
                FROM dbo.AspNetUsers au
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.Users u
                    WHERE u.Email = COALESCE(NULLIF(au.Email, N''), au.Id + N'@local')
                );
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.SportTypes', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.SportTypes
                    (
                        SportTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SportTypes PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_SportTypes_Name UNIQUE,
                        IconUrl NVARCHAR(MAX) NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_SportTypes_IsActive DEFAULT (1)
                    );
                END;

                INSERT INTO dbo.SportTypes (Name, IconUrl, IsActive)
                SELECT ct.Name, NULL, 1
                FROM dbo.CourtTypes ct
                WHERE NOT EXISTS (SELECT 1 FROM dbo.SportTypes st WHERE st.Name = ct.Name);

                IF OBJECT_ID(N'dbo.Fields', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Fields
                    (
                        FieldId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Fields PRIMARY KEY,
                        OwnerId INT NOT NULL,
                        SportTypeId INT NOT NULL,
                        Name NVARCHAR(200) NOT NULL,
                        Description NVARCHAR(MAX) NULL,
                        Address NVARCHAR(500) NOT NULL,
                        District NVARCHAR(100) NOT NULL,
                        City NVARCHAR(100) NOT NULL,
                        Latitude FLOAT NOT NULL,
                        Longitude FLOAT NOT NULL,
                        PricePerHour DECIMAL(18,0) NOT NULL,
                        OpenTime TIME NOT NULL,
                        CloseTime TIME NOT NULL,
                        SlotDurationMin INT NOT NULL CONSTRAINT DF_Fields_SlotDurationMin DEFAULT (60),
                        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Fields_Status DEFAULT (N'Pending'),
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Fields_CreatedAt DEFAULT (GETDATE()),
                        CONSTRAINT FK_Fields_Users_OwnerId FOREIGN KEY (OwnerId) REFERENCES dbo.Users(UserId),
                        CONSTRAINT FK_Fields_SportTypes_SportTypeId FOREIGN KEY (SportTypeId) REFERENCES dbo.SportTypes(SportTypeId),
                        CONSTRAINT CK_Fields_PricePerHour_Positive CHECK (PricePerHour > 0),
                        CONSTRAINT CK_Fields_CloseTime_After_OpenTime CHECK (CloseTime > OpenTime),
                        CONSTRAINT CK_Fields_SlotDurationMin_Positive CHECK (SlotDurationMin > 0),
                        CONSTRAINT CK_Fields_Status CHECK (Status IN (N'Pending', N'Active', N'Blocked'))
                    );
                    CREATE INDEX IX_Fields_OwnerId ON dbo.Fields(OwnerId);
                    CREATE INDEX IX_Fields_SportTypeId ON dbo.Fields(SportTypeId);
                    CREATE INDEX IX_Fields_Location ON dbo.Fields(City, District);
                END;

                DECLARE @FallbackOwnerId INT = COALESCE(
                    (SELECT TOP 1 u.UserId FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId WHERE r.RoleName = N'FieldOwner' ORDER BY u.UserId),
                    (SELECT TOP 1 UserId FROM dbo.Users ORDER BY UserId)
                );

                IF @FallbackOwnerId IS NOT NULL
                BEGIN
                    SET IDENTITY_INSERT dbo.Fields ON;

                    INSERT INTO dbo.Fields (FieldId, OwnerId, SportTypeId, Name, Description, Address, District, City, Latitude, Longitude, PricePerHour, OpenTime, CloseTime, SlotDurationMin, Status, CreatedAt)
                    SELECT
                        c.Id,
                        COALESCE(uOwner.UserId, @FallbackOwnerId),
                        st.SportTypeId,
                        c.Name,
                        NULLIF(c.Description, N''),
                        CASE WHEN LEN(c.Address) > 500 THEN LEFT(c.Address, 500) ELSE c.Address END,
                        CASE WHEN c.Address LIKE N'%Go Vap%' OR c.Address LIKE N'%Gò Vấp%' THEN N'Gò Vấp' ELSE N'Chưa cập nhật' END,
                        CASE WHEN c.Address LIKE N'%Ho Chi Minh%' OR c.Address LIKE N'%Hồ Chí Minh%' THEN N'TP. Hồ Chí Minh' ELSE N'Chưa cập nhật' END,
                        c.Latitude,
                        c.Longitude,
                        CASE WHEN c.HourlyPrice <= 0 THEN 1 ELSE CONVERT(DECIMAL(18,0), c.HourlyPrice) END,
                        CONVERT(TIME, '06:00:00'),
                        CONVERT(TIME, '22:00:00'),
                        60,
                        CASE WHEN c.IsActive = 1 THEN N'Active' ELSE N'Blocked' END,
                        GETDATE()
                    FROM dbo.Courts c
                    JOIN dbo.CourtTypes ct ON ct.Id = c.CourtTypeId
                    JOIN dbo.SportTypes st ON st.Name = ct.Name
                    OUTER APPLY (
                        SELECT TOP 1 u.UserId
                        FROM dbo.Users u
                        LEFT JOIN dbo.AspNetUsers au ON au.Email = u.Email
                        WHERE u.Email = c.OwnerName OR au.Id = c.OwnerId
                        ORDER BY u.UserId
                    ) uOwner
                    WHERE NOT EXISTS (SELECT 1 FROM dbo.Fields f WHERE f.FieldId = c.Id);

                    SET IDENTITY_INSERT dbo.Fields OFF;
                END;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.FieldImages', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.FieldImages
                    (
                        ImageId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FieldImages PRIMARY KEY,
                        FieldId INT NOT NULL,
                        ImageUrl NVARCHAR(MAX) NOT NULL,
                        IsPrimary BIT NOT NULL CONSTRAINT DF_FieldImages_IsPrimary DEFAULT (0),
                        SortOrder INT NOT NULL CONSTRAINT DF_FieldImages_SortOrder DEFAULT (0),
                        CONSTRAINT FK_FieldImages_Fields_FieldId FOREIGN KEY (FieldId) REFERENCES dbo.Fields(FieldId) ON DELETE CASCADE
                    );
                    CREATE INDEX IX_FieldImages_FieldId ON dbo.FieldImages(FieldId);
                END;

                INSERT INTO dbo.FieldImages (FieldId, ImageUrl, IsPrimary, SortOrder)
                SELECT c.Id, c.ImageUrl, 1, 0
                FROM dbo.Courts c
                JOIN dbo.Fields f ON f.FieldId = c.Id
                WHERE NULLIF(c.ImageUrl, N'') IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM dbo.FieldImages fi WHERE fi.FieldId = c.Id AND fi.ImageUrl = c.ImageUrl);

                IF OBJECT_ID(N'dbo.TimeSlots', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.TimeSlots
                    (
                        SlotId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TimeSlots PRIMARY KEY,
                        FieldId INT NOT NULL,
                        SlotDate DATE NOT NULL,
                        StartTime TIME NOT NULL,
                        EndTime TIME NOT NULL,
                        PriceOverride DECIMAL(18,0) NULL,
                        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_TimeSlots_Status DEFAULT (N'Available'),
                        CONSTRAINT FK_TimeSlots_Fields_FieldId FOREIGN KEY (FieldId) REFERENCES dbo.Fields(FieldId) ON DELETE CASCADE,
                        CONSTRAINT UQ_TimeSlots_Field_Date_Start UNIQUE (FieldId, SlotDate, StartTime),
                        CONSTRAINT CK_TimeSlots_EndTime_After_StartTime CHECK (EndTime > StartTime),
                        CONSTRAINT CK_TimeSlots_PriceOverride_Positive CHECK (PriceOverride IS NULL OR PriceOverride > 0),
                        CONSTRAINT CK_TimeSlots_Status CHECK (Status IN (N'Available', N'Locked', N'Booked', N'Blocked'))
                    );
                    CREATE INDEX IX_TimeSlots_FieldId ON dbo.TimeSlots(FieldId);
                    CREATE INDEX IX_TimeSlots_Date_Status ON dbo.TimeSlots(SlotDate, Status);
                END;

                INSERT INTO dbo.TimeSlots (FieldId, SlotDate, StartTime, EndTime, PriceOverride, Status)
                SELECT DISTINCT b.CourtId, CONVERT(DATE, b.BookingDate), b.StartTime, b.EndTime, NULL, N'Booked'
                FROM dbo.Bookings b
                JOIN dbo.Fields f ON f.FieldId = b.CourtId
                WHERE b.EndTime > b.StartTime
                  AND NOT EXISTS (
                      SELECT 1
                      FROM dbo.TimeSlots ts
                      WHERE ts.FieldId = b.CourtId
                        AND ts.SlotDate = CONVERT(DATE, b.BookingDate)
                        AND ts.StartTime = b.StartTime
                  );
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Bookings', N'BookingId') IS NULL
                    ALTER TABLE dbo.Bookings ADD BookingId AS Id;
                IF COL_LENGTH(N'dbo.Bookings', N'CustomerId') IS NULL
                    ALTER TABLE dbo.Bookings ADD CustomerId INT NULL;
                IF COL_LENGTH(N'dbo.Bookings', N'SlotId') IS NULL
                    ALTER TABLE dbo.Bookings ADD SlotId INT NULL;
                IF COL_LENGTH(N'dbo.Bookings', N'FieldId') IS NULL
                    ALTER TABLE dbo.Bookings ADD FieldId INT NULL;
                IF COL_LENGTH(N'dbo.Bookings', N'BookedAt') IS NULL
                    ALTER TABLE dbo.Bookings ADD BookedAt DATETIME2 NOT NULL CONSTRAINT DF_Bookings_BookedAt DEFAULT (GETDATE());
                IF COL_LENGTH(N'dbo.Bookings', N'ExpiredAt') IS NULL
                    ALTER TABLE dbo.Bookings ADD ExpiredAt DATETIME2 NOT NULL CONSTRAINT DF_Bookings_ExpiredAt DEFAULT (DATEADD(MINUTE, 10, GETDATE()));
                IF COL_LENGTH(N'dbo.Bookings', N'CancelReason') IS NULL
                    ALTER TABLE dbo.Bookings ADD CancelReason NVARCHAR(500) NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE b
                SET CustomerId = u.UserId
                FROM dbo.Bookings b
                JOIN dbo.AspNetUsers au ON au.Id = b.UserId
                JOIN dbo.Users u ON u.Email = au.Email
                WHERE b.CustomerId IS NULL;

                UPDATE b
                SET FieldId = b.CourtId
                FROM dbo.Bookings b
                JOIN dbo.Fields f ON f.FieldId = b.CourtId
                WHERE b.FieldId IS NULL;

                UPDATE b
                SET SlotId = ts.SlotId
                FROM dbo.Bookings b
                JOIN dbo.TimeSlots ts
                    ON ts.FieldId = b.CourtId
                   AND ts.SlotDate = CONVERT(DATE, b.BookingDate)
                   AND ts.StartTime = b.StartTime
                WHERE b.SlotId IS NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Bookings_BookingId' AND object_id = OBJECT_ID(N'dbo.Bookings'))
                    CREATE UNIQUE INDEX UQ_Bookings_BookingId ON dbo.Bookings(BookingId);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Bookings_SlotId_Report' AND object_id = OBJECT_ID(N'dbo.Bookings'))
                    CREATE UNIQUE INDEX UQ_Bookings_SlotId_Report ON dbo.Bookings(SlotId) WHERE SlotId IS NOT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_Users_CustomerId_Report')
                    ALTER TABLE dbo.Bookings WITH NOCHECK ADD CONSTRAINT FK_Bookings_Users_CustomerId_Report FOREIGN KEY (CustomerId) REFERENCES dbo.Users(UserId);
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_TimeSlots_SlotId_Report')
                    ALTER TABLE dbo.Bookings WITH NOCHECK ADD CONSTRAINT FK_Bookings_TimeSlots_SlotId_Report FOREIGN KEY (SlotId) REFERENCES dbo.TimeSlots(SlotId);
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Bookings_Fields_FieldId_Report')
                    ALTER TABLE dbo.Bookings WITH NOCHECK ADD CONSTRAINT FK_Bookings_Fields_FieldId_Report FOREIGN KEY (FieldId) REFERENCES dbo.Fields(FieldId);
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Payments
                    (
                        PaymentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
                        BookingId INT NOT NULL,
                        TransactionCode NVARCHAR(100) NULL CONSTRAINT UQ_Payments_TransactionCode UNIQUE,
                        Amount DECIMAL(18,0) NOT NULL,
                        Currency NVARCHAR(10) NOT NULL CONSTRAINT DF_Payments_Currency DEFAULT (N'VND'),
                        Gateway NVARCHAR(50) NOT NULL CONSTRAINT DF_Payments_Gateway DEFAULT (N'VNPay'),
                        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Payments_Status DEFAULT (N'Pending'),
                        PaidAt DATETIME2 NULL,
                        GatewayRef NVARCHAR(MAX) NULL,
                        CONSTRAINT UQ_Payments_BookingId UNIQUE (BookingId),
                        CONSTRAINT FK_Payments_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(Id) ON DELETE CASCADE,
                        CONSTRAINT CK_Payments_Amount_Positive CHECK (Amount > 0),
                        CONSTRAINT CK_Payments_Status CHECK (Status IN (N'Pending', N'Success', N'Failed', N'Refunded'))
                    );
                END;

                INSERT INTO dbo.Payments (BookingId, TransactionCode, Amount, Currency, Gateway, Status, PaidAt, GatewayRef)
                SELECT
                    b.Id,
                    NULL,
                    CONVERT(DECIMAL(18,0), CASE WHEN b.TotalAmount <= 0 THEN 1 ELSE b.TotalAmount END),
                    N'VND',
                    N'Wallet',
                    CASE WHEN b.Status IN (N'Confirmed', N'Paid', N'Completed') THEN N'Success' ELSE N'Pending' END,
                    NULL,
                    NULL
                FROM dbo.Bookings b
                WHERE NOT EXISTS (SELECT 1 FROM dbo.Payments p WHERE p.BookingId = b.Id);
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Promotions', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Promotions
                    (
                        PromoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Promotions PRIMARY KEY,
                        FieldId INT NOT NULL,
                        Code NVARCHAR(50) NOT NULL CONSTRAINT UQ_Promotions_Code UNIQUE,
                        DiscountType NVARCHAR(20) NOT NULL,
                        DiscountValue DECIMAL(18,2) NOT NULL,
                        MaxUses INT NOT NULL CONSTRAINT DF_Promotions_MaxUses DEFAULT (100),
                        UsedCount INT NOT NULL CONSTRAINT DF_Promotions_UsedCount DEFAULT (0),
                        ValidFrom DATE NOT NULL,
                        ValidTo DATE NOT NULL,
                        CONSTRAINT FK_Promotions_Fields_FieldId FOREIGN KEY (FieldId) REFERENCES dbo.Fields(FieldId) ON DELETE CASCADE,
                        CONSTRAINT CK_Promotions_DiscountType CHECK (DiscountType IN (N'Percentage', N'Fixed')),
                        CONSTRAINT CK_Promotions_DiscountValue_Positive CHECK (DiscountValue > 0),
                        CONSTRAINT CK_Promotions_MaxUses_NonNegative CHECK (MaxUses >= 0),
                        CONSTRAINT CK_Promotions_UsedCount_NonNegative CHECK (UsedCount >= 0),
                        CONSTRAINT CK_Promotions_UsedCount_MaxUses CHECK (UsedCount <= MaxUses),
                        CONSTRAINT CK_Promotions_ValidTo_After_ValidFrom CHECK (ValidTo >= ValidFrom)
                    );
                    CREATE INDEX IX_Promotions_FieldId ON dbo.Promotions(FieldId);
                END;

                DECLARE @FirstFieldId INT = (SELECT TOP 1 FieldId FROM dbo.Fields ORDER BY FieldId);
                IF @FirstFieldId IS NOT NULL
                BEGIN
                    INSERT INTO dbo.Promotions (FieldId, Code, DiscountType, DiscountValue, MaxUses, UsedCount, ValidFrom, ValidTo)
                    SELECT
                        @FirstFieldId,
                        LEFT(v.Code, 50),
                        CASE WHEN v.DiscountType IN (N'Percent', N'Percentage') THEN N'Percentage' ELSE N'Fixed' END,
                        CASE WHEN v.DiscountValue <= 0 THEN 1 ELSE v.DiscountValue END,
                        CASE WHEN v.UsageLimit < 0 THEN 0 ELSE v.UsageLimit END,
                        CASE WHEN v.UsedCount < 0 THEN 0 ELSE v.UsedCount END,
                        CONVERT(DATE, v.StartDate),
                        CONVERT(DATE, v.EndDate)
                    FROM dbo.Vouchers v
                    WHERE NOT EXISTS (SELECT 1 FROM dbo.Promotions p WHERE p.Code = LEFT(v.Code, 50));
                END;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Reviews
                    (
                        ReviewId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY,
                        BookingId INT NOT NULL,
                        FieldId INT NOT NULL,
                        UserId INT NOT NULL,
                        Rating TINYINT NOT NULL,
                        Comment NVARCHAR(MAX) NULL,
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Reviews_CreatedAt DEFAULT (GETDATE()),
                        CONSTRAINT UQ_Reviews_BookingId UNIQUE (BookingId),
                        CONSTRAINT FK_Reviews_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_Reviews_Fields_FieldId FOREIGN KEY (FieldId) REFERENCES dbo.Fields(FieldId),
                        CONSTRAINT FK_Reviews_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
                        CONSTRAINT CK_Reviews_Rating_Range CHECK (Rating BETWEEN 1 AND 5)
                    );
                    CREATE INDEX IX_Reviews_FieldId ON dbo.Reviews(FieldId);
                    CREATE INDEX IX_Reviews_UserId ON dbo.Reviews(UserId);
                END;

                INSERT INTO dbo.Reviews (BookingId, FieldId, UserId, Rating, Comment, CreatedAt)
                SELECT
                    cr.BookingId,
                    cr.CourtId,
                    u.UserId,
                    CASE WHEN cr.Rating < 1 THEN 1 WHEN cr.Rating > 5 THEN 5 ELSE CONVERT(TINYINT, cr.Rating) END,
                    cr.Comment,
                    cr.CreatedAt
                FROM dbo.CourtReviews cr
                JOIN dbo.Bookings b ON b.Id = cr.BookingId
                JOIN dbo.Fields f ON f.FieldId = cr.CourtId
                JOIN dbo.AspNetUsers au ON au.Id = cr.UserId
                JOIN dbo.Users u ON u.Email = au.Email
                WHERE NOT EXISTS (SELECT 1 FROM dbo.Reviews r WHERE r.BookingId = cr.BookingId);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Reviews', N'U') IS NOT NULL DROP TABLE dbo.Reviews;
                IF OBJECT_ID(N'dbo.Promotions', N'U') IS NOT NULL DROP TABLE dbo.Promotions;
                IF OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL DROP TABLE dbo.Payments;
                IF OBJECT_ID(N'dbo.FieldImages', N'U') IS NOT NULL DROP TABLE dbo.FieldImages;
                IF OBJECT_ID(N'dbo.TimeSlots', N'U') IS NOT NULL DROP TABLE dbo.TimeSlots;
                IF OBJECT_ID(N'dbo.Fields', N'U') IS NOT NULL DROP TABLE dbo.Fields;
                IF OBJECT_ID(N'dbo.SportTypes', N'U') IS NOT NULL DROP TABLE dbo.SportTypes;
                IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
                IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL DROP TABLE dbo.Roles;
                """);
        }
    }
}

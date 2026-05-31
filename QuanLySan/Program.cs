using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;
using QuanLySan.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, NoOpEmailSender>();
builder.Services.AddHostedService<BookingReminderService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await context.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('Courts', 'OwnerId') IS NULL
        BEGIN
            ALTER TABLE [Courts] ADD [OwnerId] nvarchar(max) NOT NULL CONSTRAINT [DF_Courts_OwnerId] DEFAULT N'';
        END

        IF COL_LENGTH('Courts', 'OwnerName') IS NULL
        BEGIN
            ALTER TABLE [Courts] ADD [OwnerName] nvarchar(max) NOT NULL CONSTRAINT [DF_Courts_OwnerName] DEFAULT N'';
        END

        IF OBJECT_ID(N'[Reports]', N'U') IS NULL
        BEGIN
            CREATE TABLE [Reports] (
                [Id] int NOT NULL IDENTITY,
                [CourtId] int NOT NULL,
                [UserId] nvarchar(max) NOT NULL,
                [UserName] nvarchar(max) NOT NULL,
                [OwnerId] nvarchar(max) NOT NULL,
                [Reason] nvarchar(120) NOT NULL,
                [Description] nvarchar(max) NOT NULL,
                [Status] nvarchar(40) NOT NULL,
                [CreatedAt] datetime2 NOT NULL,
                [ResolvedAt] datetime2 NULL,
                CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Reports_Courts_CourtId] FOREIGN KEY ([CourtId]) REFERENCES [Courts] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_Reports_CourtId] ON [Reports] ([CourtId]);
        END
        """);

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Admin", "Owner", "ChuSan", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(newAdmin, "Admin@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    var chusanEmail = "chusan@gmail.com";
    var chusanUser = await userManager.FindByEmailAsync(chusanEmail);

    if (chusanUser == null)
    {
        var newChusan = new IdentityUser
        {
            UserName = chusanEmail,
            Email = chusanEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(newChusan, "Chusan@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newChusan, "ChuSan");
        }
    }
    else if (!await userManager.IsInRoleAsync(chusanUser, "ChuSan"))
    {
        await userManager.AddToRoleAsync(chusanUser, "ChuSan");
    }

    if (chusanUser != null && !await userManager.IsInRoleAsync(chusanUser, "Owner"))
    {
        await userManager.AddToRoleAsync(chusanUser, "Owner");
    }

    var ownerEmail = "owner@gmail.com";
    var ownerUser = await userManager.FindByEmailAsync(ownerEmail);

    if (ownerUser == null)
    {
        ownerUser = new IdentityUser
        {
            UserName = ownerEmail,
            Email = ownerEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(ownerUser, "Owner@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(ownerUser, "Owner");
        }
    }
    else if (!await userManager.IsInRoleAsync(ownerUser, "Owner"))
    {
        await userManager.AddToRoleAsync(ownerUser, "Owner");
    }

    var userEmail = "user@gmail.com";
    var normalUser = await userManager.FindByEmailAsync(userEmail);

    if (normalUser == null)
    {
        normalUser = new IdentityUser
        {
            UserName = userEmail,
            Email = userEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(normalUser, "User@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(normalUser, "User");
        }
    }
    else if (!await userManager.IsInRoleAsync(normalUser, "User"))
    {
        await userManager.AddToRoleAsync(normalUser, "User");
    }

    var requiredCourtTypes = new[]
    {
        "Football",
        "Badminton",
        "Tennis",
        "Volleyball",
        "Table Tennis",
        "Handball",
        "Rugby",
        "Pickleball",
        "Baseball",
        "Hockey",
        "Cricket",
        "Softball",
        "Netball",
        "Golf"
    };

    var existingCourtTypeNames = await context.LoaiSans.Select(x => x.TenLoai).ToListAsync();
    var missingCourtTypes = requiredCourtTypes
        .Where(type => !existingCourtTypeNames.Any(existing => existing.Equals(type, StringComparison.OrdinalIgnoreCase)))
        .Select(type => new LoaiSan { TenLoai = type })
        .ToList();

    if (missingCourtTypes.Any())
    {
        context.LoaiSans.AddRange(missingCourtTypes);
        await context.SaveChangesAsync();
    }

    if (!context.Sans.Any())
    {
        var bongDa = context.LoaiSans.First(x => x.TenLoai == "Football");
        var cauLong = context.LoaiSans.First(x => x.TenLoai == "Badminton");
        var tennis = context.LoaiSans.First(x => x.TenLoai == "Tennis");

        context.Sans.AddRange(
            new San { TenSan = "Court A1 - Artificial Turf", LoaiSanId = bongDa.Id, Gia = 180000, MoTa = "New turf, LED lighting, suitable for 5-7 players.", HinhAnh = "/images/football-field.svg", TrangThai = true, DiaChi = "12 Nguyen Van Bao, Ward 4, Go Vap, Ho Chi Minh City", Latitude = 10.8222, Longitude = 106.6874, OwnerId = ownerUser?.Id ?? string.Empty, OwnerName = ownerEmail },
            new San { TenSan = "Court B2 - Badminton", LoaiSanId = cauLong.Id, Gia = 90000, MoTa = "Anti-slip floor, clean changing area.", HinhAnh = "/images/badminton-court.svg", TrangThai = true, DiaChi = "120 Le Loi, Ward 4, Go Vap, Ho Chi Minh City", Latitude = 10.8198, Longitude = 106.6805, OwnerId = ownerUser?.Id ?? string.Empty, OwnerName = ownerEmail },
            new San { TenSan = "Court C3 - Tennis", LoaiSanId = tennis.Id, Gia = 220000, MoTa = "Standard court with rest area and drinking water.", HinhAnh = "/images/tennis-court.svg", TrangThai = true, DiaChi = "50 Quang Trung, Ward 10, Go Vap, Ho Chi Minh City", Latitude = 10.8275, Longitude = 106.6718, OwnerId = ownerUser?.Id ?? string.Empty, OwnerName = ownerEmail });
        await context.SaveChangesAsync();
    }

    var existingSans = await context.Sans.ToListAsync();
    foreach (var san in existingSans)
    {
        if (string.IsNullOrWhiteSpace(san.DiaChi))
        {
            if (san.TenSan.Contains("A1") || san.TenSan.Contains("Turf") || san.TenSan.Contains("Football"))
            {
                san.DiaChi = "12 Nguyen Van Bao, Ward 4, Go Vap, Ho Chi Minh City";
                san.Latitude = 10.8222;
                san.Longitude = 106.6874;
            }
            else if (san.TenSan.Contains("B2") || san.TenSan.Contains("Badminton"))
            {
                san.DiaChi = "120 Le Loi, Ward 4, Go Vap, Ho Chi Minh City";
                san.Latitude = 10.8198;
                san.Longitude = 106.6805;
            }
            else if (san.TenSan.Contains("C3") || san.TenSan.Contains("Tennis"))
            {
                san.DiaChi = "50 Quang Trung, Ward 10, Go Vap, Ho Chi Minh City";
                san.Latitude = 10.8275;
                san.Longitude = 106.6718;
            }
            else
            {
                san.DiaChi = "12 Nguyen Van Bao, Ward 4, Go Vap, Ho Chi Minh City";
                san.Latitude = 10.8222;
                san.Longitude = 106.6874;
            }
        }

        if (string.IsNullOrWhiteSpace(san.OwnerId) && ownerUser != null)
        {
            san.OwnerId = ownerUser.Id;
            san.OwnerName = ownerEmail;
        }
    }
    if (existingSans.Any(s => context.Entry(s).State == EntityState.Modified))
    {
        await context.SaveChangesAsync();
    }

    var sansWithoutImages = await context.Sans
        .Include(x => x.LoaiSan)
        .Where(x => string.IsNullOrWhiteSpace(x.HinhAnh))
        .ToListAsync();

    foreach (var san in sansWithoutImages)
    {
        san.HinhAnh = san.LoaiSan?.TenLoai switch
        {
            "Football" => "/images/football-field.svg",
            "Badminton" => "/images/badminton-court.svg",
            "Tennis" => "/images/tennis-court.svg",
            _ => "/images/sports-field.svg"
        };
    }

    if (sansWithoutImages.Any())
    {
        await context.SaveChangesAsync();
    }

    if (!context.Vouchers.Any())
    {
        context.Vouchers.AddRange(
            new Voucher
            {
                Ma = "SPORT50",
                Ten = "Discount 50,000 VND",
                LoaiGiamGia = "Fixed",
                GiaTri = 50000,
                DonToiThieu = 150000,
                NgayBatDau = DateTime.Today.AddDays(-7),
                NgayKetThuc = DateTime.Today.AddMonths(2),
                SoLuotToiDa = 100,
                MoTa = "Áp dụng cho đơn đặt sân từ 150.000 VND."
            },
            new Voucher
            {
                Ma = "GIAM20",
                Ten = "Discount 20%",
                LoaiGiamGia = "Percent",
                GiaTri = 20,
                DonToiThieu = 200000,
                NgayBatDau = DateTime.Today.AddDays(-7),
                NgayKetThuc = DateTime.Today.AddMonths(1),
                SoLuotToiDa = 50,
                MoTa = "Ưu đãi nổi bật cho khung giờ thấp điểm."
            });
        await context.SaveChangesAsync();
    }

    if (!context.SiteSettings.Any())
    {
        context.SiteSettings.Add(new SiteSetting());
        await context.SaveChangesAsync();
    }
}

app.Run();

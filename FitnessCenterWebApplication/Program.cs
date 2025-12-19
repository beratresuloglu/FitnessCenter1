using FitnessCenterWebApplication.Data;
using FitnessCenterWebApplication.Models.Entities;
using FitnessCenterWebApplication.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 🔧 TIMEOUT VE DOSYA YÜKLEME AYARLARI (ÖNEMLİ!)
// ============================================================

// Kestrel server timeout ayarları
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5); // 5 dakika
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5); // 5 dakika
});

// IIS server ayarları (IIS kullanıyorsanız)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

// Form dosya yükleme limitleri
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// ============================================================
// Identity Ayarları
// ============================================================
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ============================================================
// Diğer Servisler
// ============================================================
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient Factory (Opsiyonel ama önerilir)
builder.Services.AddHttpClient();

var app = builder.Build();

// ============================================================
// Database Seed
// ============================================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedService.SeedDatabase(app.Services);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı seed edilirken bir hata oluştu.");
    }
}

// ============================================================
// Middleware Pipeline
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PadelSimple.Models.Data;
using PadelSimple.Models.Identity;
using PadelSimple.Web.Infrastructure;
using PadelSimple.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// MVC
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Localization (3 talen)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[]
    {
        new CultureInfo("nl"),
        new CultureInfo("en"),
        new CultureInfo("fr"),
    };

    options.DefaultRequestCulture = new RequestCulture("nl");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;

    // Eerst cookie, dan Accept-Language
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

// EF (SQL Server)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Identity met jouw AppUser/AppRole
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.SignIn.RequireConfirmedEmail = true; // EIS
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie instellingen
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/AccessDenied";
});

// App services
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>(); // eigen mail service

var app = builder.Build();

// Eigen middleware (voorbeeld: culture cookie + simpele request logging)
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<CultureCookieMiddleware>();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Localization
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Seed bij lege DB (rollen/admin/demo data)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API controllers
app.MapControllers();

app.Run();

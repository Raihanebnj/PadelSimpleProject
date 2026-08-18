using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PadelSimple.Models.Data;
using PadelSimple.Models.Identity;
using PadelSimple.Web.Middleware;
using PadelSimple.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. Controllers + Views (MVC)
// ============================================================
builder.Services.AddControllersWithViews();

// ============================================================
// 2. Meertaligheid (Localization)
// ============================================================
builder.Services.AddLocalization();

// ============================================================
// 3. SQLite Databank (migraties in het Web-project)
// ============================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("Default"),
        b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)
    ));

// ============================================================
// 4. ASP.NET Identity (Admin / Medewerker / Klant)
// ============================================================
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "PadelSimple.Auth";
});

// ============================================================
// 5. JWT Bearer Authenticatie (voor de REST API)
// ============================================================
var jwtSleutel = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT-sleutel niet geconfigureerd in appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    // Standaard: cookie (voor MVC)
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSleutel))
    };
});

builder.Services.AddAuthorization();

// ============================================================
// 6. Eigen Services
// ============================================================
builder.Services.AddScoped<AppSeeder>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// ============================================================
// 7. Logging
// ============================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ============================================================
// APPLICATIE PIPELINE
// ============================================================
var app = builder.Build();

// Foutafhandeling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Aangepaste cultuur-middleware (taalcookie verwerken)
app.UseMiddleware<LanguageCultureMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Databank migreren + seeden bij opstarten
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AppSeeder>();
    await seeder.SeedAsync();
}

// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

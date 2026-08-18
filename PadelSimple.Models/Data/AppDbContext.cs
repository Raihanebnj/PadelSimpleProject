using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;

namespace PadelSimple.Models.Data;

/// <summary>
/// Hoofd-DbContext voor de PadelSimple applicatie.
/// Erft van IdentityDbContext zodat ASP.NET Identity gebruikers en rollen worden bijgehouden.
/// </summary>
public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    // DbSets (Nederlandse namen conform opdracht)
    public DbSet<Terrein> Terreinen => Set<Terrein>();
    public DbSet<Materiaal> Materialen => Set<Materiaal>();
    public DbSet<Reservation> Reservaties => Set<Reservation>();

    // Aliassen voor achterwaartse compat. met bestaande code
    public DbSet<Terrein> Courts => Set<Terrein>();
    public DbSet<Materiaal> Equipment => Set<Materiaal>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---- Global Query Filters voor Soft-Delete ----
        builder.Entity<Terrein>().HasQueryFilter(t => !t.IsDeleted);
        builder.Entity<Materiaal>().HasQueryFilter(m => !m.IsDeleted);
        builder.Entity<Reservation>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);

        // ---- Relaties voor Reservation ----
        builder.Entity<Reservation>()
            .HasOne(r => r.Terrein)
            .WithMany(t => t.Reservaties)
            .HasForeignKey(r => r.TerreinId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reservation>()
            .HasOne(r => r.Materiaal)
            .WithMany(m => m.Reservaties)
            .HasForeignKey(r => r.MateriaalId)
            .OnDelete(DeleteBehavior.SetNull);

        // ---- Precision voor decimalen ----
        builder.Entity<Terrein>()
            .Property(t => t.Uurtarief)
            .HasColumnType("decimal(10,2)");

        builder.Entity<Materiaal>()
            .Property(m => m.Huurprijs)
            .HasColumnType("decimal(10,2)");

        builder.Entity<Reservation>()
            .Property(r => r.TotalePrijs)
            .HasColumnType("decimal(10,2)");

        // ---- Seeding ----
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // --- Rollen ---
        var adminRole = new AppRole
        {
            Id = "ROLE_ADMIN",
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "a1b2c3d4-0000-0000-0000-000000000001"
        };
        var klantRole = new AppRole
        {
            Id = "ROLE_KLANT",
            Name = "Klant",
            NormalizedName = "KLANT",
            ConcurrencyStamp = "a1b2c3d4-0000-0000-0000-000000000002"
        };
        builder.Entity<AppRole>().HasData(adminRole, klantRole);

        // --- Gebruikers ---
        var hasher = new PasswordHasher<AppUser>();

        var adminUser = new AppUser
        {
            Id = "USER_ADMIN_001",
            UserName = "admin@padelsimple.be",
            NormalizedUserName = "ADMIN@PADELSIMPLE.BE",
            Email = "admin@padelsimple.be",
            NormalizedEmail = "ADMIN@PADELSIMPLE.BE",
            EmailConfirmed = true,
            Voornaam = "Administrator",
            Achternaam = "PadelSimple",
            Telefoonnummer = "+32 498 00 00 01",
            IsLid = true,
            IsBlocked = false,
            IsDeleted = false,
            SecurityStamp = "ADMIN_SECURITY_STAMP_001",
            ConcurrencyStamp = "ADMIN_CONCURRENCY_STAMP_001"
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

        var klantUser = new AppUser
        {
            Id = "USER_KLANT_001",
            UserName = "klant@padelsimple.be",
            NormalizedUserName = "KLANT@PADELSIMPLE.BE",
            Email = "klant@padelsimple.be",
            NormalizedEmail = "KLANT@PADELSIMPLE.BE",
            EmailConfirmed = true,
            Voornaam = "Jan",
            Achternaam = "Janssen",
            Telefoonnummer = "+32 476 12 34 56",
            IsLid = true,
            IsBlocked = false,
            IsDeleted = false,
            SecurityStamp = "KLANT_SECURITY_STAMP_001",
            ConcurrencyStamp = "KLANT_CONCURRENCY_STAMP_001"
        };
        klantUser.PasswordHash = hasher.HashPassword(klantUser, "Klant123!");

        builder.Entity<AppUser>().HasData(adminUser, klantUser);

        // --- Koppeling gebruikers aan rollen ---
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = adminRole.Id },
            new IdentityUserRole<string> { UserId = klantUser.Id, RoleId = klantRole.Id }
        );

        // --- Terreinen ---
        builder.Entity<Terrein>().HasData(
            new Terrein
            {
                Id = 1,
                Naam = "Terrein 1 (Overdekt)",
                Capaciteit = 4,
                IsIndoors = true,
                Uurtarief = 18.00m,
                IsDeleted = false
            },
            new Terrein
            {
                Id = 2,
                Naam = "Terrein 2 (Buiten)",
                Capaciteit = 4,
                IsIndoors = false,
                Uurtarief = 12.00m,
                IsDeleted = false
            },
            new Terrein
            {
                Id = 3,
                Naam = "Terrein 3 (Overdekt VIP)",
                Capaciteit = 4,
                IsIndoors = true,
                Uurtarief = 25.00m,
                IsDeleted = false
            }
        );

        // --- Materialen ---
        builder.Entity<Materiaal>().HasData(
            new Materiaal
            {
                Id = 1,
                Naam = "Padelracket",
                AantalInInventaris = 20,
                Huurprijs = 5.00m,
                IsActief = true,
                IsDeleted = false
            },
            new Materiaal
            {
                Id = 2,
                Naam = "Set Ballen",
                AantalInInventaris = 30,
                Huurprijs = 2.50m,
                IsActief = true,
                IsDeleted = false
            },
            new Materiaal
            {
                Id = 3,
                Naam = "Beschermingsbril",
                AantalInInventaris = 15,
                Huurprijs = 1.50m,
                IsActief = true,
                IsDeleted = false
            }
        );

        // --- Reservaties (dummy data) ---
        builder.Entity<Reservation>().HasData(
            new Reservation
            {
                Id = 1,
                UserId = klantUser.Id,
                TerreinId = 1,
                MateriaalId = 1,
                AantalMateriaal = 2,
                Datum = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                StartUur = new TimeSpan(10, 0, 0),
                EindUur = new TimeSpan(11, 0, 0),
                TotalePrijs = 28.00m,
                AantalSpelers = 4,
                IsDeleted = false
            },
            new Reservation
            {
                Id = 2,
                UserId = klantUser.Id,
                TerreinId = 2,
                MateriaalId = null,
                AantalMateriaal = 0,
                Datum = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                StartUur = new TimeSpan(14, 0, 0),
                EindUur = new TimeSpan(15, 0, 0),
                TotalePrijs = 12.00m,
                AantalSpelers = 2,
                IsDeleted = false
            },
            new Reservation
            {
                Id = 3,
                UserId = adminUser.Id,
                TerreinId = 3,
                MateriaalId = 2,
                AantalMateriaal = 1,
                Datum = new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc),
                StartUur = new TimeSpan(18, 0, 0),
                EindUur = new TimeSpan(19, 30, 0),
                TotalePrijs = 40.00m,
                AantalSpelers = 4,
                IsDeleted = false
            }
        );
    }
}

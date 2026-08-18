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

    public DbSet<ReservationMateriaal> ReservationMaterialen => Set<ReservationMateriaal>();

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

        // ---- Relaties voor Reservation & ReservationMateriaal ----
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

        builder.Entity<ReservationMateriaal>()
            .HasOne(rm => rm.Reservation)
            .WithMany(r => r.ReservationMaterialen)
            .HasForeignKey(rm => rm.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ReservationMateriaal>()
            .HasOne(rm => rm.Materiaal)
            .WithMany()
            .HasForeignKey(rm => rm.MateriaalId)
            .OnDelete(DeleteBehavior.Restrict);

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
        var medewerkerRole = new AppRole
        {
            Id = "ROLE_MEDEWERKER",
            Name = "Medewerker",
            NormalizedName = "MEDEWERKER",
            ConcurrencyStamp = "a1b2c3d4-0000-0000-0000-000000000003"
        };
        var klantRole = new AppRole
        {
            Id = "ROLE_KLANT",
            Name = "Klant",
            NormalizedName = "KLANT",
            ConcurrencyStamp = "a1b2c3d4-0000-0000-0000-000000000002"
        };
        builder.Entity<AppRole>().HasData(adminRole, medewerkerRole, klantRole);

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

    }
}

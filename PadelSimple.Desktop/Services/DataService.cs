using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.Services;

/// <summary>
/// Service voor alle CRUD-operaties op terreinen, materialen en reservaties.
/// Gebruikt zowel LINQ Query Syntax als LINQ Method Syntax (vereiste opdracht).
/// </summary>
public class DataService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DataService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // ================================================================
    //  TERREINEN
    // ================================================================

    /// <summary>Haalt alle actieve terreinen op (LINQ Method Syntax).</summary>
    public async Task<List<Terrein>> GetTerreinen()
    {
        using var db = _contextFactory.CreateDbContext();
        return await db.Terreinen
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Naam)
            .ToListAsync();
    }

    /// <summary>Alias voor GetTerreinen – compatibiliteit met bestaande code.</summary>
    public Task<List<Terrein>> GetCourtsAsync() => GetTerreinen();

    public async Task<Terrein?> GetTerreinByIdAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        return await db.Terreinen.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task SlaTerreinOpAsync(Terrein terrein)
    {
        using var db = _contextFactory.CreateDbContext();
        try
        {
            if (terrein.Id == 0)
            {
                db.Terreinen.Add(terrein);
            }
            else
            {
                var bestaand = await db.Terreinen.FirstOrDefaultAsync(t => t.Id == terrein.Id);
                if (bestaand == null) return;
                bestaand.Naam = terrein.Naam;
                bestaand.Capaciteit = terrein.Capaciteit;
                bestaand.IsIndoors = terrein.IsIndoors;
                bestaand.Uurtarief = terrein.Uurtarief;
            }
            await db.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }

    public async Task SaveCourtAsync(Terrein terrein) => await SlaTerreinOpAsync(terrein);

    public async Task SaveCourtsAsync(IEnumerable<Terrein> terreinen)
    {
        using var db = _contextFactory.CreateDbContext();
        foreach (var t in terreinen)
        {
            if (t.Id == 0)
            {
                db.Terreinen.Add(t);
            }
            else
            {
                var bestaand = await db.Terreinen.FirstOrDefaultAsync(x => x.Id == t.Id);
                if (bestaand == null) continue;
                bestaand.Naam = t.Naam;
                bestaand.Capaciteit = t.Capaciteit;
                bestaand.IsIndoors = t.IsIndoors;
                bestaand.Uurtarief = t.Uurtarief;
            }
        }
        await db.SaveChangesAsync();
    }

    public async Task VerwijderTerreinAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        var terrein = await db.Terreinen.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id);
        if (terrein == null) return;
        terrein.IsDeleted = true;
        terrein.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // ================================================================
    //  MATERIALEN
    // ================================================================

    /// <summary>Haalt alle actieve materialen op (LINQ Query Syntax – vereiste opdracht).</summary>
    public async Task<List<Materiaal>> GetMaterialen()
    {
        using var db = _contextFactory.CreateDbContext();

        // LINQ Query Syntax (vereist in opdracht)
        var query = from m in db.Materialen
                    where m.IsActief && !m.IsDeleted
                    orderby m.Naam
                    select m;

        return await query.ToListAsync();
    }

    /// <summary>Alias voor GetMaterialen – compatibiliteit.</summary>
    public Task<List<Materiaal>> GetEquipmentAsync() => GetMaterialen();

    public async Task<Materiaal?> GetMateriaalByIdAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        return await db.Materialen.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task SlaMateriaalOpAsync(Materiaal materiaal)
    {
        using var db = _contextFactory.CreateDbContext();
        try
        {
            if (materiaal.Id == 0)
            {
                db.Materialen.Add(materiaal);
            }
            else
            {
                var bestaand = await db.Materialen.FirstOrDefaultAsync(m => m.Id == materiaal.Id);
                if (bestaand == null) return;
                bestaand.Naam = materiaal.Naam;
                bestaand.AantalInInventaris = materiaal.AantalInInventaris;
                bestaand.Huurprijs = materiaal.Huurprijs;
                bestaand.IsActief = materiaal.IsActief;
            }
            await db.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }

    public async Task SaveEquipmentAsync(Materiaal mat) => await SlaMateriaalOpAsync(mat);

    public async Task SaveEquipmentAsync(IEnumerable<Materiaal> materialenLijst)
    {
        using var db = _contextFactory.CreateDbContext();
        foreach (var m in materialenLijst)
        {
            if (m.Id == 0)
            {
                db.Materialen.Add(m);
            }
            else
            {
                var bestaand = await db.Materialen.FirstOrDefaultAsync(x => x.Id == m.Id);
                if (bestaand == null) continue;
                bestaand.Naam = m.Naam;
                bestaand.AantalInInventaris = m.AantalInInventaris;
                bestaand.Huurprijs = m.Huurprijs;
                bestaand.IsActief = m.IsActief;
            }
        }
        await db.SaveChangesAsync();
    }

    public async Task VerwijderMateriaalAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        var mat = await db.Materialen.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);
        if (mat == null) return;
        mat.IsDeleted = true;
        mat.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // ================================================================
    //  RESERVATIES
    // ================================================================

    /// <summary>
    /// Haalt reservaties op, optioneel gefilterd op datum.
    /// Combineert LINQ Method Syntax en sortering in geheugen (TimeSpan-beperking).
    /// </summary>
    public async Task<List<Reservation>> GetReservaties(DateTime? voorDatum = null)
    {
        using var db = _contextFactory.CreateDbContext();

        // LINQ Method Syntax (vereiste opdracht)
        var query = db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.User)
            .AsQueryable();

        if (voorDatum.HasValue)
            query = query.Where(r => r.Datum.Date == voorDatum.Value.Date);

        var lijst = await query.ToListAsync();

        // In-memory sortering omdat TimeSpan niet door SQLite vertaald wordt
        return lijst
            .OrderBy(r => r.Datum)
            .ThenBy(r => r.StartUur)
            .ToList();
    }

    /// <summary>Alias voor achterwaartse compatibiliteit.</summary>
    public Task<List<Reservation>> GetReservationsAsync(DateTime? forDate = null)
        => GetReservaties(forDate);

    public async Task MaakReservatieAanAsync(Reservation reservatie)
    {
        using var db = _contextFactory.CreateDbContext();
        using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            // Overlap-check: haal dezelfde-dag-reservaties voor dit terrein op (LINQ Query Syntax)
            var zelfdeTerreinReservaties =
                (from r in db.Reservaties
                 where r.TerreinId == reservatie.TerreinId
                    && r.Datum.Date == reservatie.Datum.Date
                 select r).ToList();

            bool overlap = zelfdeTerreinReservaties.Any(r =>
                r.StartUur < reservatie.EindUur &&
                reservatie.StartUur < r.EindUur);

            if (overlap)
                throw new InvalidOperationException(
                    "Er bestaat al een reservatie voor dit terrein en tijdslot.");

            // Bereken totale prijs
            var terrein = await db.Terreinen.FindAsync(reservatie.TerreinId)
                ?? throw new InvalidOperationException("Terrein niet gevonden.");
            var duur = (decimal)(reservatie.EindUur - reservatie.StartUur).TotalHours;
            reservatie.TotalePrijs = terrein.Uurtarief * duur;

            if (reservatie.MateriaalId.HasValue && reservatie.AantalMateriaal > 0)
            {
                var mat = await db.Materialen.FindAsync(reservatie.MateriaalId.Value)
                    ?? throw new InvalidOperationException("Materiaal niet gevonden.");
                reservatie.TotalePrijs += mat.Huurprijs * reservatie.AantalMateriaal;
            }

            db.Reservaties.Add(reservatie);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public Task CreateReservationAsync(Reservation r) => MaakReservatieAanAsync(r);

    public async Task WijzigReservatieAsync(Reservation reservatie)
    {
        using var db = _contextFactory.CreateDbContext();
        try
        {
            var bestaand = await db.Reservaties.FindAsync(reservatie.Id);
            if (bestaand == null) return;

            bestaand.TerreinId = reservatie.TerreinId;
            bestaand.MateriaalId = reservatie.MateriaalId;
            bestaand.AantalMateriaal = reservatie.AantalMateriaal;
            bestaand.Datum = reservatie.Datum;
            bestaand.StartUur = reservatie.StartUur;
            bestaand.EindUur = reservatie.EindUur;
            bestaand.AantalSpelers = reservatie.AantalSpelers;

            // Herbereken prijs
            var terrein = await db.Terreinen.FindAsync(reservatie.TerreinId);
            if (terrein != null)
            {
                var duur = (decimal)(reservatie.EindUur - reservatie.StartUur).TotalHours;
                bestaand.TotalePrijs = terrein.Uurtarief * duur;

                if (reservatie.MateriaalId.HasValue && reservatie.AantalMateriaal > 0)
                {
                    var mat = await db.Materialen.FindAsync(reservatie.MateriaalId.Value);
                    if (mat != null)
                        bestaand.TotalePrijs += mat.Huurprijs * reservatie.AantalMateriaal;
                }
            }

            await db.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }

    public async Task SoftDeleteReservatieAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        var res = await db.Reservaties.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);
        if (res == null) return;
        res.IsDeleted = true;
        res.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public Task SoftDeleteReservationAsync(int id) => SoftDeleteReservatieAsync(id);

    public async Task<List<Reservation>> GetReservatiesVanGebruiker(string userId)
    {
        using var db = _contextFactory.CreateDbContext();

        // LINQ Query Syntax (vereiste opdracht)
        var query = from r in db.Reservaties
                    where r.UserId == userId
                    orderby r.Datum descending, r.StartUur
                    select r;

        var lijst = await query
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .ToListAsync();

        return lijst;
    }
}
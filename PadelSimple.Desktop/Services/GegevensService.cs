using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.Services;

/// <summary>
/// Service voor alle CRUD-operaties op terreinen, materialen en reservaties.
/// Bevat strikte voorraad- en overlapvalidatie en ondersteunt meerdere materialen per reservatie.
/// Gebruikt zowel LINQ Query Syntax als LINQ Method Syntax (vereiste opdracht).
/// </summary>
public class GegevensService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public GegevensService(IDbContextFactory<AppDbContext> contextFactory)
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
            .Where(t => !t.IsVerwijderd)
            .OrderBy(t => t.Naam)
            .ToListAsync();
    }

    public async Task<Terrein?> GetTerreinOpIdAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        return await db.Terreinen.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task SlaTerreinOpAsync(Terrein terrein)
    {
        using var db = _contextFactory.CreateDbContext();
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

    public async Task SlaTerreinenOpAsync(IEnumerable<Terrein> terreinen)
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
        terrein.IsVerwijderd = true;
        terrein.VerwijderdOp = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // ================================================================
    //  MATERIALEN
    // ================================================================

    /// <summary>Haalt alle actieve materialen op (LINQ Query Syntax – vereiste opdracht).</summary>
    public async Task<List<Materiaal>> GetMaterialen()
    {
        using var db = _contextFactory.CreateDbContext();

        // LINQ Query Syntax
        var query = from m in db.Materialen
                    where m.IsActief && !m.IsVerwijderd
                    orderby m.Naam
                    select m;

        return await query.ToListAsync();
    }

    public async Task<Materiaal?> GetMateriaalOpIdAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        return await db.Materialen.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task SlaMateriaalOpAsync(Materiaal materiaal)
    {
        using var db = _contextFactory.CreateDbContext();
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

    public async Task SlaMaterialenOpAsync(IEnumerable<Materiaal> materialenLijst)
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
        mat.IsVerwijderd = true;
        mat.VerwijderdOp = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // ================================================================
    //  RESERVATIES
    // ================================================================

    /// <summary>
    /// Haalt reservaties op inclusief terrein, materialen en gebruiker.
    /// </summary>
    public async Task<List<Reservatie>> GetReservaties(DateTime? voorDatum = null)
    {
        using var db = _contextFactory.CreateDbContext();

        // LINQ Method Syntax
        var query = db.Reservaties
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.Gebruiker)
            .Include(r => r.ReservatieMaterialen)
                .ThenInclude(rm => rm.Materiaal)
            .AsQueryable();

        if (voorDatum.HasValue)
            query = query.Where(r => r.Datum.Date == voorDatum.Value.Date);

        var lijst = await query.ToListAsync();

        return lijst
            .OrderBy(r => r.Datum)
            .ThenBy(r => r.StartUur)
            .ToList();
    }

    /// <summary>
    /// Maakt een nieuwe reservatie aan met strikte terrein-overlap en materiaal-voorraadcontrole.
    /// </summary>
    public async Task MaakReservatieAanAsync(Reservatie reservatie, List<(int MateriaalId, int Aantal)> gekozenMaterialen)
    {
        using var db = _contextFactory.CreateDbContext();
        using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            // 1. Check terrein overlap op dezelfde datum (LINQ Query Syntax)
            var zelfdeTerreinReservaties =
                (from r in db.Reservaties
                 where r.TerreinId == reservatie.TerreinId
                    && r.Datum.Date == reservatie.Datum.Date
                    && !r.IsVerwijderd
                 select r).ToList();

            bool terreinOverlap = zelfdeTerreinReservaties.Any(r =>
                r.StartUur < reservatie.EindUur &&
                reservatie.StartUur < r.EindUur);

            if (terreinOverlap)
                throw new InvalidOperationException("Er bestaat al een reservatie voor dit terrein en tijdslot.");

            // 2. Strikte materiaalvoorraad- en overlapcontrole
            await ValideerMateriaalStockAsync(db, reservatie, gekozenMaterialen);

            // 3. Bereken totale prijs
            var terrein = await db.Terreinen.FindAsync(reservatie.TerreinId)
                ?? throw new InvalidOperationException("Terrein niet gevonden.");
            var duur = (decimal)(reservatie.EindUur - reservatie.StartUur).TotalHours;
            reservatie.TotalePrijs = terrein.Uurtarief * duur;

            // Voeg gekozen materialen toe aan reservatie
            reservatie.ReservatieMaterialen.Clear();
            foreach (var (matId, aantal) in gekozenMaterialen)
            {
                if (aantal > 0)
                {
                    var mat = await db.Materialen.FindAsync(matId);
                    if (mat != null)
                    {
                        reservatie.TotalePrijs += mat.Huurprijs * aantal;
                        reservatie.ReservatieMaterialen.Add(new ReservatieMateriaal
                        {
                            MateriaalId = matId,
                            Aantal = aantal
                        });
                    }
                }
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

    public async Task WijzigReservatieAsync(Reservatie reservatie, List<(int MateriaalId, int Aantal)> gekozenMaterialen)
    {
        using var db = _contextFactory.CreateDbContext();
        using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var bestaand = await db.Reservaties
                .Include(r => r.ReservatieMaterialen)
                .FirstOrDefaultAsync(r => r.Id == reservatie.Id);
            if (bestaand == null) throw new InvalidOperationException("Reservatie niet gevonden.");

            // 1. Check terrein overlap
            var zelfdeTerreinReservaties = await db.Reservaties
                .Where(r => r.Id != reservatie.Id
                         && r.TerreinId == reservatie.TerreinId
                         && r.Datum.Date == reservatie.Datum.Date
                         && !r.IsVerwijderd)
                .ToListAsync();

            bool terreinOverlap = zelfdeTerreinReservaties.Any(r =>
                r.StartUur < reservatie.EindUur && reservatie.StartUur < r.EindUur);

            if (terreinOverlap)
                throw new InvalidOperationException("Er bestaat al een reservatie voor dit terrein op dit tijdslot.");

            // 2. Materialen valideer stock
            await ValideerMateriaalStockAsync(db, reservatie, gekozenMaterialen);

            bestaand.TerreinId = reservatie.TerreinId;
            bestaand.Datum = reservatie.Datum;
            bestaand.StartUur = reservatie.StartUur;
            bestaand.EindUur = reservatie.EindUur;
            bestaand.AantalSpelers = reservatie.AantalSpelers;

            // Verwijder oude koppelingen
            db.ReservatieMaterialen.RemoveRange(bestaand.ReservatieMaterialen);

            // Herbereken prijs en voeg nieuwe gekozen materialen toe
            var terrein = await db.Terreinen.FindAsync(reservatie.TerreinId);
            var duur = (decimal)(reservatie.EindUur - reservatie.StartUur).TotalHours;
            bestaand.TotalePrijs = (terrein?.Uurtarief ?? 0m) * duur;

            foreach (var (matId, aantal) in gekozenMaterialen)
            {
                if (aantal > 0)
                {
                    var mat = await db.Materialen.FindAsync(matId);
                    if (mat != null)
                    {
                        bestaand.TotalePrijs += mat.Huurprijs * aantal;
                        bestaand.ReservatieMaterialen.Add(new ReservatieMateriaal
                        {
                            ReservatieId = bestaand.Id,
                            MateriaalId = matId,
                            Aantal = aantal
                        });
                    }
                }
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Strikte controle op materiaalvoorraad (zowel totale inventaris als tijds-overlap).
    /// </summary>
    private async Task ValideerMateriaalStockAsync(AppDbContext db, Reservatie reservatie, List<(int MateriaalId, int Aantal)> gekozenMaterialen)
    {
        foreach (var (matId, gevraagdAantal) in gekozenMaterialen)
        {
            if (gevraagdAantal <= 0) continue;

            var mat = await db.Materialen.FindAsync(matId)
                ?? throw new InvalidOperationException("Materiaal niet gevonden.");

            // A) Controleer of gevraagd aantal de totale inventaris overschrijdt
            if (gevraagdAantal > mat.AantalInInventaris)
            {
                throw new InvalidOperationException(
                    $"Niet genoeg voorraad van '{mat.Naam}'.\n" +
                    $"Totale inventaris: {mat.AantalInInventaris} stuks, gevraagd: {gevraagdAantal} stuks.");
            }

            // B) Controleer overlappende reservaties op dezelfde datum & tijdslot
            var overlappendeReservaties = await db.Reservaties
                .Include(r => r.ReservatieMaterialen)
                .Where(r => r.Id != reservatie.Id
                         && r.Datum.Date == reservatie.Datum.Date
                         && !r.IsVerwijderd)
                .ToListAsync();

            // Filter op tijdsoverlap
            var inTijdsOverlap = overlappendeReservaties.Where(r =>
                r.StartUur < reservatie.EindUur && reservatie.StartUur < r.EindUur).ToList();

            int alGereserveerd = 0;
            foreach (var r in inTijdsOverlap)
            {
                if (r.ReservatieMaterialen != null && r.ReservatieMaterialen.Count > 0)
                {
                    alGereserveerd += r.ReservatieMaterialen
                        .Where(rm => rm.MateriaalId == matId)
                        .Sum(rm => rm.Aantal);
                }
                else if (r.MateriaalId == matId)
                {
                    alGereserveerd += r.AantalMateriaal;
                }
            }

            if (alGereserveerd + gevraagdAantal > mat.AantalInInventaris)
            {
                int nogBeschikbaar = Math.Max(0, mat.AantalInInventaris - alGereserveerd);
                throw new InvalidOperationException(
                    $"Onvoldoende voorraad van '{mat.Naam}' op het gekozen tijdstip ({reservatie.StartUur:hh\\:mm} - {reservatie.EindUur:hh\\:mm}).\n" +
                    $"Reeds gereserveerd op dit tijdstip: {alGereserveerd} van de {mat.AantalInInventaris}.\n" +
                    $"Nog beschikbaar op dit tijdstip: {nogBeschikbaar}, gevraagd: {gevraagdAantal}.");
            }
        }
    }

    public async Task VerwijderReservatieZachtAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();
        var res = await db.Reservaties.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);
        if (res == null) return;
        res.IsVerwijderd = true;
        res.VerwijderdOp = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Haalt enkel de reservaties op van een specifieke gebruiker (LINQ Query Syntax).
    /// </summary>
    public async Task<List<Reservatie>> GetReservatiesVanGebruiker(string gebruikersId)
    {
        using var db = _contextFactory.CreateDbContext();

        // LINQ Query Syntax (server-side filtering)
        var query = from r in db.Reservaties
                    where r.GebruikerId == gebruikersId && !r.IsVerwijderd
                    orderby r.Datum descending
                    select r;

        var lijst = await query
            .Include(r => r.Terrein)
            .Include(r => r.Materiaal)
            .Include(r => r.ReservatieMaterialen)
                .ThenInclude(rm => rm.Materiaal)
            .ToListAsync();

        // Client-side in-memory sortering op TimeSpan (voorkomt SQLite TimeSpan ORDER BY beperking)
        return lijst
            .OrderByDescending(r => r.Datum)
            .ThenBy(r => r.StartUur)
            .ToList();
    }
}
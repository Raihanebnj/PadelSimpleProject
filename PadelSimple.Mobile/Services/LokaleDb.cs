using PadelSimple.Models.Dtos;
using SQLite;

namespace PadelSimple.Mobile.Services;

public class LokaleDb
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public LokaleDb()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "padelsimple.mobile.db");
        _db = new SQLiteAsyncConnection(path);
    }

    private async Task InitAsync()
    {
        if (_initialized) return;

        await _db.CreateTableAsync<LokaleReservatieInWacht>();
        await _db.CreateTableAsync<LokaleGecachteTerrein>();
        await _db.CreateTableAsync<LokaleGecachtMateriaal>();
        await _db.CreateTableAsync<LokaleGecachteReservatie>();

        _initialized = true;
    }
    public async Task<List<LokaleReservatieInWacht>> GetPendingAsync()
    {
        await InitAsync();
        return await _db.Table<LokaleReservatieInWacht>().ToListAsync();
    }

    public async Task InsertPendingAsync(LokaleReservatieInWacht item)
    {
        await InitAsync();
        await _db.InsertAsync(item);
    }

    public async Task DeletePendingAsync(int id)
    {
        await InitAsync();
        await _db.DeleteAsync<LokaleReservatieInWacht>(id);
    }
    
    public async Task ReplaceTerreinenAsync(IEnumerable<TerreinDto> terreinen)
    {
        await InitAsync();
        await _db.DeleteAllAsync<LokaleGecachteTerrein>();
        foreach (var c in terreinen)
        {
            await _db.InsertAsync(new LokaleGecachteTerrein
            {
                Id = c.Id,
                Naam = c.Naam,
                Capaciteit = c.Capaciteit,
                IsIndoors = c.IsIndoors
            });
        }
    }

    public async Task<List<TerreinDto>> GetGecachteTerreinenAsync()
    {
        await InitAsync();
        var rows = await _db.Table<LokaleGecachteTerrein>().ToListAsync();
        return rows
            .Select(r => new TerreinDto(r.Id, r.Naam, r.Capaciteit, r.IsIndoors))
            .ToList();
    }

    public async Task ReplaceMateriaalAsync(IEnumerable<MateriaalDto> items)
    {
        await InitAsync();
        await _db.DeleteAllAsync<LokaleGecachtMateriaal>();
        foreach (var e in items)
        {
            await _db.InsertAsync(new LokaleGecachtMateriaal
            {
                Id = e.Id,
                Naam = e.Naam,
                AantalInInventaris = e.AantalInInventaris,
                BeschikbaarAantal = e.BeschikbaarAantal,
                IsActief = e.IsActief
            });
        }
    }

    public async Task<List<MateriaalDto>> GetGecachtMateriaalAsync()
    {
        await InitAsync();
        var rows = await _db.Table<LokaleGecachtMateriaal>().ToListAsync();
        return rows
            .Select(r => new MateriaalDto(r.Id, r.Naam, r.AantalInInventaris, r.BeschikbaarAantal, r.IsActief))
            .ToList();
    }

    public async Task ReplaceReservatiesAsync(DateTime date, IEnumerable<ReservatieDto> items)
    {
        await InitAsync();
        await _db.ExecuteAsync("DELETE FROM LokaleGecachteReservatie WHERE Datum = ?", date.Date);
        foreach (var r in items)
        {
            await _db.InsertAsync(new LokaleGecachteReservatie
            {
                Id = r.Id,
                TerreinId = r.TerreinId,
                TerreinNaam = r.TerreinNaam,
                Datum = r.Datum.Date,
                StartUur = r.StartUur,
                EindUur = r.EindUur,
                AantalSpelers = r.AantalSpelers,
                MateriaalId = r.MateriaalId,
                MateriaalNaam = r.MateriaalNaam,
                MateriaalAantal = r.MateriaalAantal
            });
        }
    }

    public async Task<List<ReservatieDto>> GetGecachteReservatiesAsync(DateTime date)
    {
        await InitAsync();
        var rows = await _db
            .Table<LokaleGecachteReservatie>()
            .Where(x => x.Datum == date.Date)
            .ToListAsync();
        return rows
            .Select(r => new ReservatieDto(
                r.Id,
                r.TerreinId,
                r.TerreinNaam,
                r.Datum,
                r.StartUur,
                r.EindUur,
                r.AantalSpelers,
                r.MateriaalId,
                r.MateriaalNaam,
                r.MateriaalAantal))
            .ToList();
    }
}

public class LokaleReservatieInWacht
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int TerreinId { get; set; }
    public DateTime Datum { get; set; }

    // opslaan als tekst voor simpliciteit
    public string StartUur { get; set; } = "";
    public string EindUur { get; set; } = "";

    public int AantalSpelers { get; set; }
    public int? MateriaalId { get; set; }
    public int? MateriaalAantal { get; set; }
}

public class LokaleGecachteTerrein
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Naam { get; set; } = "";
    public int Capaciteit { get; set; }
    public bool IsIndoors { get; set; }
}

public class LokaleGecachtMateriaal
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Naam { get; set; } = "";
    public int AantalInInventaris { get; set; }
    public int BeschikbaarAantal { get; set; }
    public bool IsActief { get; set; }
}

public class LokaleGecachteReservatie
{
    [PrimaryKey]
    public int Id { get; set; }
    public int TerreinId { get; set; }
    public string TerreinNaam { get; set; } = "";
    public DateTime Datum { get; set; }
    public TimeSpan StartUur { get; set; }
    public TimeSpan EindUur { get; set; }
    public int AantalSpelers { get; set; }
    public int? MateriaalId { get; set; }
    public string? MateriaalNaam { get; set; }
    public int? MateriaalAantal { get; set; }
}

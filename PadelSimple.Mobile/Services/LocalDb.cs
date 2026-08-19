using SQLite;

namespace PadelSimple.Mobile.Services;

public class LocalDb
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public LocalDb()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "padelsimple.mobile.db");
        _db = new SQLiteAsyncConnection(path);
    }

    private async Task InitAsync()
    {
        if (!_initialized)
        {
            await _db.CreateTableAsync<LocalReservationPending>();
            _initialized = true;
        }
    }

    public async Task<List<LocalReservationPending>> GetPendingAsync()
    {
        await InitAsync();
        return await _db.Table<LocalReservationPending>().ToListAsync();
    }

    public async Task InsertPendingAsync(LocalReservationPending item)
    {
        await InitAsync();
        await _db.InsertAsync(item);
    }

    public async Task DeletePendingAsync(int id)
    {
        await InitAsync();
        await _db.DeleteAsync<LocalReservationPending>(id);
    }
}

public class LocalReservationPending
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int CourtId { get; set; }
    public DateTime Date { get; set; }

    // opslaan als tekst voor simpliciteit
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";

    public int NumberOfPlayers { get; set; }
    public int? EquipmentId { get; set; }
    public int? EquipmentQuantity { get; set; }
}

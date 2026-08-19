using PadelSimple.Models.Common;
using PadelSimple.Models.Identity;

namespace PadelSimple.Models.Domain;

/// <summary>
/// Een reservatie voor een terrein, optioneel met meerdere stuks materiaal.
/// </summary>
public class Reservation : ISoftDeletable
{
    public int Id { get; set; }

    // FK naar ApplicationUser
    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;

    // FK naar Terrein
    public int TerreinId { get; set; }
    public Terrein Terrein { get; set; } = null!;

    // Optionele legacy FK naar 1 Materiaal (achterwaartse compatibiliteit)
    public int? MateriaalId { get; set; }
    public Materiaal? Materiaal { get; set; }
    public int AantalMateriaal { get; set; }

    // Meerdere materialen per reservatie
    public ICollection<ReservationMateriaal> ReservationMaterialen { get; set; } = new List<ReservationMateriaal>();

    /// <summary>Datum van de reservatie.</summary>
    public DateTime Datum { get; set; }

    /// <summary>Starttijd van de reservatie.</summary>
    public TimeSpan StartUur { get; set; }

    /// <summary>Eindtijd van de reservatie.</summary>
    public TimeSpan EindUur { get; set; }

    /// <summary>Totale prijs inclusief terrein en materiaal.</summary>
    public decimal TotalePrijs { get; set; }

    /// <summary>Aantal spelers.</summary>
    public int AantalSpelers { get; set; } = 2;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // --- Weergave helper voor DataGrid ---
    public string MateriaalSamenvatting
    {
        get
        {
            if (ReservationMaterialen != null && ReservationMaterialen.Count > 0)
            {
                return string.Join(", ", ReservationMaterialen.Select(rm => $"{rm.Materiaal?.Naam ?? "Materiaal"} ({rm.Aantal}x)"));
            }
            if (Materiaal != null && AantalMateriaal > 0)
            {
                return $"{Materiaal.Naam} ({AantalMateriaal}x)";
            }
            return "Geen";
        }
    }

    // --- Aliassen voor bestaande code ---
    public int CourtId
    {
        get => TerreinId;
        set => TerreinId = value;
    }
    public Terrein Court
    {
        get => Terrein;
        set => Terrein = value;
    }
    public int? EquipmentId
    {
        get => MateriaalId;
        set => MateriaalId = value;
    }
    public Materiaal? Equipment
    {
        get => Materiaal;
        set => Materiaal = value;
    }
    public int? EquipmentQuantity
    {
        get => AantalMateriaal;
        set => AantalMateriaal = value ?? 0;
    }
    public DateTime Date
    {
        get => Datum;
        set => Datum = value;
    }
    public TimeSpan StartTime
    {
        get => StartUur;
        set => StartUur = value;
    }
    public TimeSpan EndTime
    {
        get => EindUur;
        set => EindUur = value;
    }
    public int NumberOfPlayers
    {
        get => AantalSpelers;
        set => AantalSpelers = value;
    }

    public string CourtName => Terrein?.Naam ?? $"Terrein #{TerreinId}";
    public string? EquipmentName => Materiaal?.Naam ?? (MateriaalId.HasValue ? $"Materiaal #{MateriaalId}" : null);
}

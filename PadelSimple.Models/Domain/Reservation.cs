using PadelSimple.Models.Common;
using PadelSimple.Models.Identity;

namespace PadelSimple.Models.Domain;

/// <summary>
/// Een reservatie voor een terrein, optioneel met materiaal.
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

    // Optionele FK naar Materiaal
    public int? MateriaalId { get; set; }
    public Materiaal? Materiaal { get; set; }

    /// <summary>Aantal gehuurde stuks materiaal.</summary>
    public int AantalMateriaal { get; set; }

    /// <summary>Datum van de reservatie.</summary>
    public DateTime Datum { get; set; }

    /// <summary>Starttijd van de reservatie.</summary>
    public TimeSpan StartUur { get; set; }

    /// <summary>Eindtijd van de reservatie.</summary>
    public TimeSpan EindUur { get; set; }

    /// <summary>Totale prijs inclusief terrein en materiaal.</summary>
    public decimal TotalePrijs { get; set; }

    /// <summary>Aantal spelers (voor achterwaartse compat.).</summary>
    public int AantalSpelers { get; set; } = 2;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

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
}

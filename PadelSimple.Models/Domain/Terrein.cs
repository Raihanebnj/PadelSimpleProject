using PadelSimple.Models.Common;

namespace PadelSimple.Models.Domain;

/// <summary>
/// Vertegenwoordigt een padelterrein in de club.
/// </summary>
public class Terrein : ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>Naam van het terrein, bijv. "Terrein 1 (Overdekt)".</summary>
    public string Naam { get; set; } = string.Empty;

    /// <summary>Maximaal aantal spelers (standaard 4).</summary>
    public int Capaciteit { get; set; } = 4;

    /// <summary>Geeft aan of het terrein overdekt is.</summary>
    public bool IsIndoors { get; set; }

    /// <summary>Tarief per uur in euro.</summary>
    public decimal Uurtarief { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigatie
    public ICollection<Reservation> Reservaties { get; set; } = new List<Reservation>();

    // Aliassen met getters en setters voor compatibiliteit met Web/Mobile projecten
    public string Name
    {
        get => Naam;
        set => Naam = value;
    }

    public int Capacity
    {
        get => Capaciteit;
        set => Capaciteit = value;
    }

    public bool IsIndoor
    {
        get => IsIndoors;
        set => IsIndoors = value;
    }
}

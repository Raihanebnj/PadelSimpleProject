using PadelSimple.Models.Common;

namespace PadelSimple.Models.Domain;

/// <summary>
/// Materiaal dat te huur is via de padelclub.
/// </summary>
public class Materiaal : ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>Naam van het materiaal, bijv. "Padelracket", "Set Ballen".</summary>
    public string Naam { get; set; } = string.Empty;

    /// <summary>Totaal aantal stuks in inventaris.</summary>
    public int AantalInInventaris { get; set; }

    /// <summary>Aantal momenteel beschikbaar.</summary>
    public int AvailableQuantity { get; set; }

    /// <summary>Huurprijs per stuk per reservatie.</summary>
    public decimal Huurprijs { get; set; }

    /// <summary>Of het materiaal actief beschikbaar is.</summary>
    public bool IsActief { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigatie
    public ICollection<Reservation> Reservaties { get; set; } = new List<Reservation>();

    // Aliassen met getters en setters voor achterwaartse compatibiliteit
    public string Name
    {
        get => Naam;
        set => Naam = value;
    }

    public int TotalQuantity
    {
        get => AantalInInventaris;
        set => AantalInInventaris = value;
    }

    public bool IsActive
    {
        get => IsActief;
        set => IsActief = value;
    }
}

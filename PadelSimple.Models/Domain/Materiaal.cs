using System.ComponentModel.DataAnnotations;
using PadelSimple.Models.Common;

namespace PadelSimple.Models.Domain;

/// <summary>
/// Materiaal dat te huur is via de padelclub.
/// </summary>
public class Materiaal : ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>Naam van het materiaal, bijv. "Padelracket", "Set Ballen".</summary>
    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn.")]
    [Display(Name = "Naam")]
    public string Naam { get; set; } = string.Empty;

    /// <summary>Totaal aantal stuks in inventaris.</summary>
    [Range(0, 9999, ErrorMessage = "Aantal in inventaris moet tussen 0 en 9999 zijn.")]
    [Display(Name = "Totaal in inventaris")]
    public int AantalInInventaris { get; set; }

    /// <summary>Aantal momenteel beschikbaar.</summary>
    [Range(0, 9999, ErrorMessage = "Beschikbaar aantal moet tussen 0 en 9999 zijn.")]
    [Display(Name = "Beschikbaar")]
    public int AvailableQuantity { get; set; }

    /// <summary>Huurprijs per stuk per reservatie.</summary>
    [Range(0.00, 999.99, ErrorMessage = "Huurprijs moet tussen € 0,00 en € 999,99 zijn.")]
    [Display(Name = "Huurprijs (€/stuk)")]
    public decimal Huurprijs { get; set; }

    /// <summary>Of het materiaal actief beschikbaar is.</summary>
    [Display(Name = "Actief")]
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

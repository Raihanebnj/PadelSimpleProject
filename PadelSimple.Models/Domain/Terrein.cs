using System.ComponentModel.DataAnnotations;
using PadelSimple.Models.Common;

namespace PadelSimple.Models.Domain;

/// <summary>
/// Vertegenwoordigt een padelterrein in de club.
/// </summary>
public class Terrein : IZachtVerwijderbaar
{
    public int Id { get; set; }

    /// <summary>Naam van het terrein, bijv. "Terrein 1 (Overdekt)".</summary>
    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn.")]
    [Display(Name = "Naam")]
    public string Naam { get; set; } = string.Empty;

    /// <summary>Maximaal aantal spelers (standaard 4).</summary>
    [Range(1, 10, ErrorMessage = "Capaciteit moet tussen 1 en 10 zijn.")]
    [Display(Name = "Capaciteit")]
    public int Capaciteit { get; set; } = 4;

    /// <summary>Geeft aan of het terrein overdekt is.</summary>
    [Display(Name = "Overdekt")]
    public bool IsIndoors { get; set; }

    /// <summary>Tarief per uur in euro.</summary>
    [Range(0.01, 500.00, ErrorMessage = "Uurtarief moet tussen € 0,01 en € 500,00 zijn.")]
    [Display(Name = "Uurtarief (€/u)")]
    public decimal Uurtarief { get; set; }

    // Soft delete
    public bool IsVerwijderd { get; set; }
    public DateTime? VerwijderdOp { get; set; }

    // Navigatie
    public ICollection<Reservatie> Reservaties { get; set; } = new List<Reservatie>();
}

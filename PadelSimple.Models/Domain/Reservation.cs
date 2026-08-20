using PadelSimple.Models.Common;
using PadelSimple.Models.Identity;

namespace PadelSimple.Models.Domain;

/// <summary>
/// Een reservatie voor een terrein, optioneel met meerdere stuks materiaal.
/// </summary>
public class Reservatie : IZachtVerwijderbaar
{
    public int Id { get; set; }

    // FK naar ApplicationUser
    public string GebruikerId { get; set; } = null!;
    public AppGebruiker Gebruiker { get; set; } = null!;

    // FK naar Terrein
    public int TerreinId { get; set; }
    public Terrein Terrein { get; set; } = null!;

    // Optionele legacy FK naar 1 Materiaal
    public int? MateriaalId { get; set; }
    public Materiaal? Materiaal { get; set; }
    public int AantalMateriaal { get; set; }

    // Meerdere materialen per reservatie
    public ICollection<ReservatieMateriaal> ReservatieMaterialen { get; set; } = new List<ReservatieMateriaal>();

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
    public bool IsVerwijderd { get; set; }
    public DateTime? VerwijderdOp { get; set; }

    // --- Weergave helper ---
    public string MateriaalSamenvatting
    {
        get
        {
            if (ReservatieMaterialen != null && ReservatieMaterialen.Count > 0)
            {
                return string.Join(", ", ReservatieMaterialen.Select(rm => $"{rm.Materiaal?.Naam ?? "Materiaal"} ({rm.Aantal}x)"));
            }
            if (Materiaal != null && AantalMateriaal > 0)
            {
                return $"{Materiaal.Naam} ({AantalMateriaal}x)";
            }
            return "Geen";
        }
    }

    public string TerreinNaam => Terrein?.Naam ?? $"Terrein #{TerreinId}";
    public string? MateriaalNaam => Materiaal?.Naam ?? (MateriaalId.HasValue ? $"Materiaal #{MateriaalId}" : null);
}

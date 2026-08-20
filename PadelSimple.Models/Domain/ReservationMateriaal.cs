namespace PadelSimple.Models.Domain;

/// <summary>
/// Koppeltabel voor meerdere gehuurde materialen per reservatie met aantal.
/// </summary>
public class ReservatieMateriaal
{
    public int Id { get; set; }

    public int ReservatieId { get; set; }
    public Reservatie? Reservatie { get; set; }

    public int MateriaalId { get; set; }
    public Materiaal Materiaal { get; set; } = null!;

    public int Aantal { get; set; }
}

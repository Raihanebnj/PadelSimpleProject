namespace PadelSimple.Models.Domain;

/// <summary>
/// Koppeltabel voor meerdere gehuurde materialen per reservatie met aantal.
/// </summary>
public class ReservationMateriaal
{
    public int Id { get; set; }

    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public int MateriaalId { get; set; }
    public Materiaal Materiaal { get; set; } = null!;

    public int Aantal { get; set; }
}

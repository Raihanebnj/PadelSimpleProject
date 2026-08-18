namespace PadelSimple.Web.ViewModels.Gebruikers;

public class GebruikerDetailsVm
{
    public string Id { get; set; } = string.Empty;
    public string VolledigeNaam { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefoon { get; set; } = string.Empty;
    public bool IsLid { get; set; }
    public bool IsGeblokkeerd { get; set; }
    public bool EmailBevestigd { get; set; }
    public List<string> Rollen { get; set; } = new();
    public List<ReservatieRijVm> Reservaties { get; set; } = new();
    public List<string> BeschikbareRollen { get; set; } = new();
}

public class ReservatieRijVm
{
    public int Id { get; set; }
    public DateTime Datum { get; set; }
    public TimeSpan StartUur { get; set; }
    public TimeSpan EindUur { get; set; }
    public string TerreinNaam { get; set; } = string.Empty;
    public decimal TotalePrijs { get; set; }
}

namespace PadelSimple.Web.ViewModels.Gebruikers;

public class GebruikerIndexVm
{
    public List<GebruikerRijVm> Gebruikers { get; set; } = new();
    public string? ZoekTerm { get; set; }
    public string? RolFilter { get; set; }
    public List<string> BeschikbareRollen { get; set; } = new();
}

public class GebruikerRijVm
{
    public string Id { get; set; } = string.Empty;
    public string VolledigeNaam { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsLid { get; set; }
    public bool IsGeblokkeerd { get; set; }
    public bool EmailBevestigd { get; set; }
    public List<string> Rollen { get; set; } = new();
}

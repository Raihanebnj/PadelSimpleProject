namespace PadelSimple.Web.ViewModels.Terreinen;

public class TerreinRijVm
{
    public int Id { get; set; }
    public string Naam { get; set; } = string.Empty;
    public int Capaciteit { get; set; }
    public bool IsIndoors { get; set; }
    public decimal Uurtarief { get; set; }
    public bool IsBeschikbaar { get; set; }
    public string? VrijVanaf { get; set; }
}

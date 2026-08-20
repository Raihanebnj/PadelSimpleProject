using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels.Terreinen;

public class TerreinenOverzichtVm
{
    public DateTime Datum { get; set; } = DateTime.Today;
    public string? Start { get; set; }
    public string? Einde { get; set; }

    public List<TerreinRijVm> Terreinen { get; set; } = new();
}

public class TerreinEditVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn.")]
    [Display(Name = "Naam van het terrein")]
    public string Naam { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Capaciteit moet tussen 1 en 10 zijn.")]
    [Display(Name = "Capaciteit (spelers)")]
    public int Capaciteit { get; set; } = 4;

    [Display(Name = "Overdekt terrein (indoors)")]
    public bool IsIndoors { get; set; }

    [Range(0.01, 500.00, ErrorMessage = "Uurtarief moet tussen € 0,01 en € 500,00 zijn.")]
    [Display(Name = "Uurtarief (€/u)")]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal Uurtarief { get; set; } = 18.00m;
}

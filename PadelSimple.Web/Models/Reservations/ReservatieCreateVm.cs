using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels;

public class ReservatieCreateVm
{
    [Required]
    public DateTime Datum { get; set; }

    [Required]
    public TimeSpan StartUur { get; set; }

    [Required]
    public TimeSpan EindUur { get; set; }

    [Required]
    public int TerreinId { get; set; }

    public int? MateriaalId { get; set; }
    public int? MateriaalAantal { get; set; }

    [Range(1, 4)]
    public int AantalSpelers { get; set; } = 2;
    public List<SelectListItem> Terreinen { get; set; } = new();
    public List<SelectListItem> Materialen { get; set; } = new();
}

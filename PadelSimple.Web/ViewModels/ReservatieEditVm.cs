using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels;

public class ReservatieEditVm
{
    public int? Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Datum { get; set; } = DateTime.Today;

    // user typt "18:00"
    [Required]
    [RegularExpression(@"^\d{1,2}:\d{2}$", ErrorMessage = "Tijd moet bv. 18:00 zijn.")]
    public string StartUur { get; set; } = "";

    [Required]
    [RegularExpression(@"^\d{1,2}:\d{2}$", ErrorMessage = "Tijd moet bv. 19:00 zijn.")]
    public string EindUur { get; set; } = "";

    [Required]
    public int TerreinId { get; set; }

    public int? MateriaalId { get; set; }

    [Range(0, 999)]
    public int AantalMateriaal { get; set; }

    [Range(1, 12)]
    public int AantalSpelers { get; set; } = 2;

    public List<SelectListItem> Terreinen { get; set; } = new();
    public List<SelectListItem> Materialen { get; set; } = new();
}

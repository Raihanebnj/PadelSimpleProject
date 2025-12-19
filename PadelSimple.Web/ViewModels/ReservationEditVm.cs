using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels;

public class ReservationEditVm
{
    public int? Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    // user typt "18:00"
    [Required]
    [RegularExpression(@"^\d{1,2}:\d{2}$", ErrorMessage = "Tijd moet bv. 18:00 zijn.")]
    public string StartTime { get; set; } = "";

    [Required]
    [RegularExpression(@"^\d{1,2}:\d{2}$", ErrorMessage = "Tijd moet bv. 19:00 zijn.")]
    public string EndTime { get; set; } = "";

    [Required]
    public int CourtId { get; set; }

    public int? EquipmentId { get; set; }

    [Range(0, 999)]
    public int EquipmentQuantity { get; set; }

    [Range(1, 12)]
    public int NumberOfPlayers { get; set; } = 2;

    public List<SelectListItem> Courts { get; set; } = new();
    public List<SelectListItem> Equipment { get; set; } = new();
}

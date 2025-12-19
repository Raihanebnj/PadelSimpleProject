using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels;

public class ReservationCreateVm
{
    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public int CourtId { get; set; }

    public int? EquipmentId { get; set; }
    public int? EquipmentQuantity { get; set; }

    [Range(1, 4)]
    public int NumberOfPlayers { get; set; } = 2;
    public List<SelectListItem> Courts { get; set; } = new();
    public List<SelectListItem> Equipment { get; set; } = new();
}

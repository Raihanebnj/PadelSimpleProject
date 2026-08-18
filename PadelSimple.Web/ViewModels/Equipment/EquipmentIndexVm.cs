using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels.Equipment;

public class EquipmentIndexVm
{
    public List<EquipmentRowVm> Items { get; set; } = new();
}

public class EquipmentRowVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal Huurprijs { get; set; }
    public bool IsActive { get; set; }
}

public class MateriaalEditVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn.")]
    [Display(Name = "Naam van materiaal")]
    public string Naam { get; set; } = string.Empty;

    [Range(0, 9999, ErrorMessage = "Aantal moet tussen 0 en 9999 zijn.")]
    [Display(Name = "Aantal in inventaris")]
    public int Aantal { get; set; } = 10;

    [Range(0.00, 999.99, ErrorMessage = "Huurprijs moet tussen € 0,00 en € 999,99 zijn.")]
    [Display(Name = "Huurprijs (€/stuk)")]
    public decimal Huurprijs { get; set; } = 2.50m;

    [Display(Name = "Actief beschikbaar")]
    public bool IsActief { get; set; } = true;
}

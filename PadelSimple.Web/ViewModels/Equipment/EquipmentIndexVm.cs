using System.Collections.Generic;

namespace PadelSimple.Web.ViewModels.Equipment;

public class EquipmentIndexVm
{
    public List<EquipmentRowVm> Items { get; set; } = new();
}

public class EquipmentRowVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsActive { get; set; }
}

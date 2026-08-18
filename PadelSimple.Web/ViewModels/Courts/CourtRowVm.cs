namespace PadelSimple.Web.ViewModels.Courts;

public class CourtRowVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsIndoor { get; set; }
    public decimal Uurtarief { get; set; }
    public bool IsAvailable { get; set; }
    public string? FreeFrom { get; set; }
}

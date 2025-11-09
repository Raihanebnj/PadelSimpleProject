namespace PadelSimple.Models.Domain;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
}

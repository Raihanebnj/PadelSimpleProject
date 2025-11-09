namespace PadelSimple.Models.Domain;

public class Court
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Capacity { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
}

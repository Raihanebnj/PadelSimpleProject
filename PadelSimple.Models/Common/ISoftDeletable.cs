namespace PadelSimple.Models.Common;

/// <summary>
/// Interface voor soft-delete ondersteuning op entiteiten.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

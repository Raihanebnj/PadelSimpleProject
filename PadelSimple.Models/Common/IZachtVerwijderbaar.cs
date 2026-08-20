namespace PadelSimple.Models.Common;

/// <summary>
/// Interface voor soft-delete ondersteuning op entiteiten.
/// </summary>
public interface IZachtVerwijderbaar
{
    bool IsVerwijderd { get; set; }
    DateTime? VerwijderdOp { get; set; }
}

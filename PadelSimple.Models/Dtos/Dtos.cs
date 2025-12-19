namespace PadelSimple.Models.Dtos;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token);

public record CourtDto(int Id, string Name, int Capacity, bool IsIndoor);

public record EquipmentDto(int Id, string Name, int TotalQuantity, int AvailableQuantity, bool IsActive);

public record ReservationCreateDto(
    int CourtId,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int NumberOfPlayers,
    int? EquipmentId,
    int? EquipmentQuantity
);

public record ReservationDto(
    int Id,
    int CourtId,
    string CourtName,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int NumberOfPlayers,
    int? EquipmentId,
    string? EquipmentName,
    int? EquipmentQuantity
);

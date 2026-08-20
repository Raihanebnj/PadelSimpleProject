namespace PadelSimple.Models.Dtos;

public record LoginAanvraag(string Email, string Wachtwoord);
public record LoginAntwoord(string Token);

public record TerreinDto(int Id, string Naam, int Capaciteit, bool IsIndoors);

public record MateriaalDto(int Id, string Naam, int AantalInInventaris, int BeschikbaarAantal, bool IsActief);

public record ReservatieCreateDto(
    int TerreinId,
    DateTime Datum,
    TimeSpan StartUur,
    TimeSpan EindUur,
    int AantalSpelers,
    int? MateriaalId,
    int? MateriaalAantal
);

public record ReservatieDto(
    int Id,
    int TerreinId,
    string TerreinNaam,
    DateTime Datum,
    TimeSpan StartUur,
    TimeSpan EindUur,
    int AantalSpelers,
    int? MateriaalId,
    string? MateriaalNaam,
    int? MateriaalAantal
);

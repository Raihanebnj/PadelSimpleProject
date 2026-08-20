# 🎾 PadelSimple — Club & Reservatiebeheer (.NET 9.0)

PadelSimple is een uitgebreid, modern beheersysteem voor padelclubs. Het helpt clubs bij het beheren van terreinen, materiaalvoorraad, reservaties en gebruikers.

---

## 🏛️ Projectstructuur

De solution bestaat uit drie hoofdprojecten:

1. **`PadelSimple.Models`** (Class Library):
   - Entiteiten (`Terrein`, `Materiaal`, `Reservation`, `ReservationMateriaal`) met Data Annotations.
   - Identity Modellen (`AppUser`, `AppRole`).
   - Soft-Delete ondersteuning (`ISoftDeletable`).
   - Global DbContext (`AppDbContext`) met EF Core Seed Data.

2. **`PadelSimple.Desktop`** (WPF .NET 9.0 Desktop App):
   - MVVM architectuur met CommunityToolkit.Mvvm.
   - Snel beheer van reservaties en voorraad via een desktop interface.

3. **`PadelSimple.Web`** (ASP.NET Core MVC + REST API):
   - Multilingual Razor Frontend (Bootstrap 5).
   - Asynchrone verwerking (`async`/`await`) & gestructureerde logging (`ILogger`).
   - RESTful API endpoints met JSON responses & JWT Bearer token ondersteuning.

---

## 👥 Identity & Rollen

De applicatie kent drie actieve rollen:
- **`Admin`**: Volledige rechten op terreinen, materialen, alle reservaties en gebruikersbeheer (rollen toewijzen, blokkeren).
- **`Medewerker`**: Kan alle terreinen, materialen en reservaties inzien en beheren, alsook gebruikers blokkeren/deblokkeren.
- **`Klant`**: Standaardrol bij registratie. Kan enkel eigen reservaties bekijken en aanmaken.

### Standaard Inloggegevens (Seed Data)
- **Admin**: `admin@padelsimple.be` / `Admin123!`
- **Medewerker**: `medewerker@padelsimple.be` / `Medewerker123!`
- **Klant**: `klant@padelsimple.be` / `Klant123!`

---

## 🔌 RESTful API Endpoints

Alle API endpoints zijn beschikbaar onder `/api/`:

| Methode | Endpoint | Omschrijving | Autorisatie |
|---|---|---|---|
| `POST` | `/api/auth/login` | Authenticeer en ontvang een JWT Bearer Token | Public |
| `POST` | `/api/auth/register` | Registreer een nieuw klantaccount en ontvang een JWT | Public |
| `GET` | `/api/terreinen` | Ophalen van alle actieve terreinen | Public |
| `GET` | `/api/terreinen/{id}` | Ophalen van specifiek terrein | Public |
| `POST` | `/api/terreinen` | Nieuw terrein toevoegen | Admin, Medewerker |
| `PUT` | `/api/terreinen/{id}` | Terrein bijwerken | Admin, Medewerker |
| `DELETE` | `/api/terreinen/{id}` | Terrein verwijderen (soft-delete) | Admin |
| `GET` | `/api/materiaal` | Ophalen van al het verhuurbare materiaal | Public |
| `GET` | `/api/materiaal/{id}` | Ophalen van specifiek materiaal | Public |
| `POST` | `/api/materiaal` | Nieuw materiaal toevoegen | Admin, Medewerker |
| `PUT` | `/api/materiaal/{id}` | Materiaal bijwerken | Admin, Medewerker |
| `DELETE` | `/api/materiaal/{id}` | Materiaal verwijderen | Admin |
| `GET` | `/api/reservaties` | Ophalen van reservaties (met optionele `?datum=` filter) | Bearer / Cookie |
| `GET` | `/api/reservaties/{id}` | Ophalen van specifieke reservatie | Bearer / Cookie |
| `POST` | `/api/reservaties` | Nieuwe reservatie aanmaken (inclusief overlap & stock validatie) | Bearer / Cookie |
| `DELETE` | `/api/reservaties/{id}` | Reservatie annuleren | Bearer / Cookie |

---

## 🌐 Meertaligheid & Middleware

- **Ondersteunde Talen**: Nederlands (`nl`), Engels (`en`), Frans (`fr`).
- **Vertalingen**: Beheerd via `.resx` bestanden in `Resources/SharedResources.{taal}.resx`.
- **LanguageCultureMiddleware**: Aangepaste middleware die de `lang` cookie uitleest en automatisch de `CultureInfo` per request instelt.
- **Taalkeuze menu**: Dynamische dropdown in de navigatiebalk met vlaggen.

---

## ⚡ AJAX Implementaties

1. **Live Tijdsloten Controle (`/Reservations/Create`)**:
   - Bij het kiezen van een terain en datum voert de browser een asynchrone `fetch()` call uit naar `/Reservations/GetBezetteTijdslots`.
   - Reeds gereserveerde uren worden direct als rode badges getoond zonder de pagina te herladen.
2. **Live Voorraadaanpassing (`/Equipment/Index`)**:
   - Beheerders kunnen de totaalvoorraad van een materiaal direct aanpassen via een AJAX `POST` call naar `/Equipment/UpdateStockAsync`.

---

## ⚙️ Installatie & Opstarten

1. **Vereisten**: .NET 9.0 SDK installed.
2. **Web-applicatie starten**:
   ```bash
   dotnet run --project PadelSimple.Web/PadelSimple.Web.csproj
   ```
3. Open de browser op `https://localhost:5001` (of de in de console aangegeven poort).

---

## 📜 Licentie & AI Vermelding

- **Licentie**: Vrij te gebruiken voor onderwijs- en evaluatiedoeleinden (Erasmushogeschool Brussel).
- **AI-vermelding**: Dit project is mede tot stand gekomen met ondersteuning van AI-coding assistenten conform de EHB CopyWrite richtlijnen. All code updates and business rules have been verified for correctness.

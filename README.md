# 🎾 PadelSimple — Club & Reservatiebeheer (.NET 9.0)

PadelSimple is een uitgebreid, modern beheersysteem voor padelclubs. Het biedt een complete oplossing voor het beheren van terreinen, materiaalvoorraad, reservaties en gebruikers via een Web-applicatie, RESTful API, Desktop (WPF) app en Mobiele (.NET MAUI) app.

---

## 🏛️ Projectstructuur

De solution bestaat uit **vier** hoofdprojecten:

1. **`PadelSimple.Models`** (Class Library - .NET 9.0):
   - **Domeinmodellen**: `Terrein`, `Materiaal`, `Reservation`, `ReservationMateriaal`.
   - **Identity Modellen**: `AppUser`, `AppRole`.
   - **Soft-Delete Ondersteuning**: `ISoftDeletable` interface & automatische query filters.
   - **Database & Seeding**: Entity Framework Core met SQLite (`AppDbContext`), inclusief automatische seed data voor gebruikers, rollen, terreinen en materialen.
   - **DTOs**: Data Transfer Objects voor REST API & client communicatie.

2. **`PadelSimple.Web`** (ASP.NET Core 9.0 MVC + REST API):
   - **Web Frontend**: MVC Razor views met een modern, responsive Bootstrap 5 design.
   - **RESTful API**: API controllers (`/api/authenticatie`, `/api/terreinen`, `/api/materiaal`, `/api/reservaties`) met JWT Bearer Token authenticatie & Swagger UI ondersteuning.
   - **Meertaligheid**: Meertalige ondersteuning (NL, EN, FR) via custom `LanguageCultureMiddleware` en `.resx` resourcebestanden.
   - **Asynchrone Functionaliteit**: Interactieve AJAX calls voor live beschikbaarheid van terreinen en voorraadaanpassingen.

3. **`PadelSimple.Desktop`** (WPF .NET 9.0 Desktop App):
   - **Architectuur**: MVVM-patroon met `CommunityToolkit.Mvvm` en Dependency Injection.
   - **Beheerdersdashboard**: Snel beheer van reservaties, terreinen, materiaalvoorraad en gebruikersaccounts voor Admins en Medewerkers.
   - **UI Componenten**: Ingebouwde dialogen en live data-binding voor administratieve taken.

4. **`PadelSimple.Mobile`** (.NET MAUI 9.0 Cross-Platform Mobile App):
   - **Architectuur**: MVVM-patroon met `CommunityToolkit.Mvvm`.
   - **Ondersteunde Platformen**: Android, iOS, macOS (MacCatalyst) en Windows Desktop.
   - **Functionaliteiten**: Mobiel inloggen, registreren, raadplegen en boeken van terreinen en verhuurmateriaal via de REST API.

---

## 👥 Identity, Rollen & Autorisatie

De applicatie kent drie rollen met specifieke toegangsrechten:
- **`Admin`**: Volledige beheerdersrechten op terreinen, materialen, alle klantreservaties en gebruikersbeheer (rollen toewijzen, gebruikers blokkeren/deblokkeren).
- **`Medewerker`**: Kan alle terreinen, materialen en reservaties inzien en beheren, alsook gebruikers blokkeren/deblokkeren.
- **`Klant`**: Standaardrol bij registratie. Kan beschikbare terreinen en materialen bekijken en eigen reservaties beheren en aanmaken.

### Standaard Inloggegevens (Seed Data)
| Rol | E-mailadres | Wachtwoord |
|---|---|---|
| **Admin** | `admin@padelsimple.be` | `Admin123!` |
| **Medewerker** | `medewerker@padelsimple.be` | `Medewerker123!` |
| **Klant** | `klant@padelsimple.be` | `Klant123!` |

---

## 🔌 RESTful API Endpoints

Alle API endpoints zijn beschikbaar onder het pad `/api/`:

| Methode | Endpoint | Omschrijving | Autorisatie |
|---|---|---|---|
| `POST` | `/api/authenticatie/login` | Authenticeer en ontvang een JWT Bearer Token | Public |
| `POST` | `/api/authenticatie/register` | Registreer een nieuw klantaccount en ontvang een JWT Token | Public |
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
| `POST` | `/api/reservaties` | Nieuwe reservatie aanmaken (inclusief overlap- en stockvalidatie) | Bearer / Cookie |
| `DELETE` | `/api/reservaties/{id}` | Reservatie annuleren | Bearer / Cookie |

---

## 🌐 Meertaligheid & Middleware

- **Ondersteunde Talen**: Nederlands (`nl`), Engels (`en`), Frans (`fr`).
- **Vertalingen**: Beheerd via resourcebestanden in `PadelSimple.Web/Resources/SharedResources.{taal}.resx`.
- **LanguageCultureMiddleware**: Aangepaste middleware die de `lang` cookie uitleest en automatisch de `CultureInfo` per request instelt.
- **Taalkeuze menu**: Dynamische dropdown met vlaggen in de navigatiebalk.

---

## ⚡ AJAX & Interactiviteit

1. **Live Tijdsloten Controle (`/Reservaties/Create`)**:
   - Bij het kiezen van een terrein en datum voert de browser een asynchrone `fetch()` call uit naar `/Reservaties/GetBezetteTijdslots`.
   - Reeds gereserveerde uren worden direct als rode badges getoond zonder de pagina te herladen.
2. **Live Voorraadaanpassing (`/Materiaal/Index`)**:
   - Beheerders kunnen de totaalvoorraad van materiaal direct aanpassen via een AJAX `POST` call naar `/Materiaal/UpdateStockAsync`.

---

## ⚙️ Installatie & Opstarten

### Vereisten
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 (v17.12 of nieuwer) met de volgende Workloads:
  - ASP.NET and web development
  - .NET Desktop Development (WPF)
  - .NET Multi-platform App UI development (.NET MAUI)

### 1. Web-applicatie & REST API starten
```bash
dotnet run --project PadelSimple.Web/PadelSimple.Web.csproj
```
- Open de browser op `https://localhost:5001` (of `http://localhost:5000`).
- API documentatie (Swagger UI) is toegankelijk via `https://localhost:5001/swagger`.

### 2. Desktop-applicatie starten (WPF)
```bash
dotnet run --project PadelSimple.Desktop/PadelSimple.Desktop.csproj
```

### 3. Mobiele applicatie starten (.NET MAUI)
```bash
# Uitvoeren op Windows:
dotnet run --project PadelSimple.Mobile/PadelSimple.Mobile.csproj -f net9.0-windows10.0.19041.0

# Uitvoeren op Android (met actieve emulator):
dotnet build PadelSimple.Mobile/PadelSimple.Mobile.csproj -t:Run -f net9.0-android
```

---

## 📜 Licentie & AI Vermelding

- **Licentie**: Vrij te gebruiken voor onderwijs- en evaluatiedoeleinden (Erasmushogeschool Brussel).
- **AI-vermelding**: Dit project is mede tot stand gekomen met ondersteuning van AI-coding assistenten conform de EHB CopyWrite richtlijnen. Alle code en logica zijn gecontroleerd en geverifieerd.

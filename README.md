# 🎾 PadelSimple

PadelSimple is een WPF desktopapplicatie ontwikkeld voor padelclubs om eenvoudig **reservaties**, **terreinen**, **materiaal** en **gebruikersbeheer** te organiseren.  
Zowel **leden** als **niet-leden** kunnen terreinen reserveren, en materiaalbeschikbaarheid wordt automatisch beheerd.

---

## ✅ Functionaliteiten

| Domein | Mogelijkheden |
|-------|---------------|
| **Gebruikers** | Registreren, aanmelden, afmelden, rollenbeheer (Admin / Manager / Member) |
| **Reservaties** | Een gebruiker kan een terrein reserveren binnen een tijdslot, optioneel met materiaal |
| **Terreinen** | CRUD-bewerkingen (toevoegen, wijzigen, soft-delete) |
| **Materiaal (Equipment)** | Inventaris en beschikbaarheid wordt automatisch beheerd |
| **Beveiliging** | Identity Framework + Role-based UI en toegang |
| **Soft Delete** | Records worden nooit fysiek verwijderd, enkel gemarkeerd |
| **Databank Seeding** | Dummy-data + Administrator account automatisch aangemaakt |

---

## 🏗️ Technologieën

| Technologie | Gebruik |
|------------|---------|
| **.NET 9 / WPF** | Desktop UI (XAML + MVVM) |
| **Entity Framework Core** | ORM + Migrations + SQLite database |
| **ASP.NET Core Identity** | Gebruikers & Rollenbeheer |
| **SQLite** | Relationale lokale databank |
| **MVVM** | Scheiding tussen UI en logica |
| **Dependency Injection** | Beheer van services en viewmodels |

---

## 🗄️ Databasemodel (vereisten voldaan)

| Tabel | Beschrijving | Relaties |
|------|--------------|----------|
| **AspNetUsers (AppUser)** | Gebruikers + IsClubMember | 1-* met Reservations |
| **Court** | Terrein met capaciteit | 1-* met Reservations |
| **Equipment** | Materiaalvoorraad | 1-* met Reservations |
| **Reservation** | Koppelt User + Court + (optioneel) Equipment + Tijd | FK UserId, CourtId, EquipmentId |

**Soft Delete toegepast op elke hoofd-entiteit.**

---

## ▶️ Installatie & Uitvoering

### 1. Clone project
```bash
git clone https://github.com/RaihaneBnj/PadelSimpleProject.git

2. Update database

In Visual Studio → Package Manager Console:

Update-Database -Project PadelSimple.Model -StartupProject PadelSimple.Desktop

3. Start de applicatie
F5

Aanmeldgegevens (Seed gebruiker)
Rol	Email	Wachtwoord
Admin	admin@padelsimple.local
	Admin!12345

🎨 UI Kenmerken

MVVM binding

Menu gebaseerd op gebruikersrol

Extra popup venster voor beheer

Styles in XAML

Selectievelden (ComboBox) voor Users, Courts en Equipment

DataGrids voor overzichtsweergaves

🧑‍⚖️ Licenties & Copyright

Alle programmatiecode is zelf geschreven, met respect voor copywritingregels.

Gebruikte libraries volgen MIT / Apache licenties (EF Core, Identity, SQLite).

Geen code gekopieerd uit ongevalideerde externe bronnen.

🤖 Gebruik van AI-tools

Ik heb ChatGPT en/of GitHub Copilot gebruikt voor:

Uitleg & debugging

Structuurplanning & documentatie

Herformulering en foutopsporing

📜 Auteur

Naam: Raihane Benjilali
Opleiding: Graduaat Programmeren – EHB
Project: Individueel eindproject C# WPF

# PadelSimple 🏓

> **Beheersapplicatie voor padelclubs** — reservaties, terreinen en materiaal in één overzichtelijke WPF-desktop-app.

---

## 📋 Inhoudsopgave

1. [Projectbeschrijving](#-projectbeschrijving)
2. [Features](#-features)
3. [Projectstructuur](#-projectstructuur)
4. [Vereisten](#-vereisten)
5. [Installatie en opstarten](#-installatie-en-opstarten)
6. [Standaard aanmeldgegevens](#-standaard-aanmeldgegevens)
7. [Technologieën](#-technologieën)
8. [AI-verantwoording (EHB CopyWrite)](#-ai-verantwoording-ehb-copywrite)
9. [Licentie](#-licentie)

---

## 📖 Projectbeschrijving

**PadelSimple** is een WPF .NET 9.0 bureaubladapplicatie die ontwikkeld werd als oefenproject voor condities en iteraties (EHB). De applicatie stelt een padelclub in staat om:

- Reservaties van klanten te beheren (aanmaken, bewerken, verwijderen)
- Terreinen en materialen te beheren als administrator
- Gebruikersaccounts te beheren (blokkeren, rollen toewijzen)

Klanten kunnen zelf een account aanmaken, optioneel een lidmaatschap nemen en reservaties plaatsen voor terreinen met of zonder gehuurde uitrusting.

---

## ✅ Features

### Authenticatie
- Inloggen op basis van e-mail of gebruikersnaam
- Registratie met voornaam, achternaam, telefoonnummer en optioneel lidmaatschap
- Geblokkeerde accounts worden geweigerd bij aanmelding
- Rollen: **Admin** en **Klant**

### Reservatiebeheer (Klanten & Admin)
- Volledige CRUD: aanmaken, bewerken en verwijderen (soft-delete)
- ComboBox-selectie voor **Terrein** en optioneel **Materiaal**
- Automatische prijsberekening (uurtarief × uren + materiaal × aantal)
- Overlap-controle: dubbele boekingen worden geweigerd

### Beheerschermen (Admin only)
- **Terreinen**: naam, capaciteit, overdekt/buiten, uurtarief — inline bewerken
- **Materialen**: naam, inventaris, huurprijs, actief — inline bewerken
- **Gebruikers**: e-mail, lidmaatschap, geblokkeerd — Admin-rol toewijzen/verwijderen, blokkeren/deblokkeren

### Extra technische vereisten (voldaan)
| Vereiste | Implementatie |
|---|---|
| 3+ containertypen | `Grid`, `StackPanel`, `DockPanel`, `WrapPanel` |
| Centrale XAML-stijlen | `App.xaml`: buttons, textboxen, datagrids, labels, … |
| XAML Data Binding | `Binding Path=...` doorheen alle views |
| LINQ Query Syntax | `DataService.GetMaterialen()`, `GetReservatiesVanGebruiker()` |
| LINQ Method Syntax | `DataService.GetTerreinen()`, `GetReservaties()` |
| Extra Window (popup) | `TerreinWindow.xaml`, `ReservationDialog.xaml` |
| Custom UserControl | `StatusBadge` (toont groen/grijs badge op basis van status) |
| Try-catch + MessageBox | Alle database-acties hebben foutafhandeling |
| Soft-delete | Global Query Filter op alle entiteiten |

---

## 🗂️ Projectstructuur

```
PadelSimpleProject/
├── PadelSimple.Models/          # Class Library – datamodellen en database
│   ├── Common/
│   │   └── ISoftDeletable.cs
│   ├── Identity/
│   │   ├── AppUser.cs           # IdentityUser + Voornaam, Achternaam, IsLid, …
│   │   └── AppRole.cs
│   ├── Domain/
│   │   ├── Terrein.cs
│   │   ├── Materiaal.cs
│   │   └── Reservation.cs
│   └── Data/
│       ├── AppDbContext.cs      # DbContext + seeding + query filters
│       └── AppDbContextFactory.cs
│
└── PadelSimple.Desktop/         # WPF Desktop – UI en logica
    ├── App.xaml / App.xaml.cs
    ├── Services/
    │   ├── AuthService.cs       # Login, registratie, gebruikersbeheer
    │   └── DataService.cs       # CRUD terreinen, materialen, reservaties
    ├── VieuwModels/
    │   ├── LoginViewModel.cs
    │   ├── MainViewModel.cs
    │   └── ReservationDialogViewModel.cs
    ├── Views/
    │   ├── LoginWindow.xaml     # Aanmelden + Registratie (met Lid-checkbox)
    │   ├── MainWindow.xaml      # TabControl: Reservaties | Mijn Account | Beheer
    │   ├── ReservationDialog.xaml   # Popup: reservatie aanmaken/bewerken
    │   ├── TerreinWindow.xaml   # Popup: terrein aanmaken/bewerken
    │   └── Controls/
    │       ├── StatusBadge.xaml # Custom UserControl: gekleurde badge
    │       └── Badge.xaml       # Generieke badge
    └── Converters/
        └── BoolToVisibilityConverter.cs  # + 3 andere converters
```

---

## 🔧 Vereisten

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 (v17.8+) met **Desktop development with .NET** workload
- NuGet pakketten worden automatisch hersteld bij eerste build

---

## 🚀 Installatie en opstarten

1. **Clone de repository**
   ```bash
   git clone https://github.com/<jouw-repo>/PadelSimpleProject.git
   cd PadelSimpleProject
   ```

2. **Migraties toepassen** (optioneel – de app doet dit ook automatisch bij opstarten)
   ```bash
   cd PadelSimple.Models
   dotnet ef database update --startup-project ../PadelSimple.Desktop
   ```

3. **Applicatie starten**
   ```
   Open PadelSimple.sln in Visual Studio
   Stel PadelSimple.Desktop als startup-project in
   Druk F5
   ```

   Of via terminal:
   ```bash
   cd PadelSimple.Desktop
   dotnet run
   ```

De SQLite-databank wordt automatisch aangemaakt in:
`%AppData%\PadelSimple\padelsimple.db`

---

## 🔑 Standaard aanmeldgegevens

| Rol | E-mail | Wachtwoord |
|---|---|---|
| **Admin** | admin@padelsimple.be | `Admin123!` |
| **Klant** | klant@padelsimple.be | `Klant123!` |

---

## 🛠️ Technologieën

| Technologie | Versie | Gebruik |
|---|---|---|
| .NET | 9.0 | Target framework |
| WPF | - | UI framework |
| Entity Framework Core | 9.0.10 | ORM / database |
| SQLite | - | Database |
| ASP.NET Core Identity | 9.0.10 | Authenticatie en rollen |
| CommunityToolkit.Mvvm | 8.4.0 | ObservableObject, RelayCommand |
| Microsoft.Extensions.Hosting | 9.0.10 | Dependency Injection / DI |

---

## 🤖 AI-verantwoording (EHB CopyWrite)

> **Conform de EHB CopyWrite-richtlijnen voor AI-gebruik bij opdrachten.**

Dit project werd deels gerealiseerd met behulp van **AI-assistentie** (Google Deepmind Antigravity / Claude Sonnet). De AI werd ingezet voor:

- Het genereren van boilerplate XAML-structuren
- Het opstellen van de AppDbContext-seeding
- Suggesties voor LINQ-queries en service-methoden
- Opmaak van de README

**Alle gegenereerde code werd gecontroleerd, aangepast en begrepen** door de student alvorens op te leveren. De functionele logica, keuze van structuur en implementatiebeslissingen zijn de verantwoordelijkheid van de student.

**Gebruikte AI-tool:** Google Deepmind Antigravity (Claude Sonnet 4.6 Thinking)
**Datum:** augustus 2026
**Student:** [Voornaam Achternaam] — EHB, opleiding Toegepaste Informatica

---

## 📄 Licentie

```
MIT License

Copyright (c) 2026 PadelSimple Project Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

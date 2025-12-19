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

----------------------------------------------

# 🏓 PadelSimple Web

PadelSimple is een **ASP.NET Core MVC webapplicatie** voor het beheren van **padelterreinen, reservaties, materiaal en gebruikers**, met **rolgebaseerde toegang (Admin / User)** en een **SQLite database**.

---

## ✨ Functionaliteiten

### 👤 Gebruikers
- Registreren
- Inloggen / Uitloggen
- Rolgebaseerde toegang:
  - **Admin**: volledig beheer
  - **User**: reservaties bekijken en maken

### 📅 Reservaties
- Terreinen reserveren per **datum & tijdslot**
- Materiaal toevoegen aan reservaties
- Automatisch zien **wanneer een terrein weer vrij is**

### 🏟️ Terreinen
- Indoor / Outdoor
- Capaciteit
- Beschikbaarheid per geselecteerd tijdslot

### 🎒 Materiaal
- Totale en beschikbare hoeveelheid
- Actief / Inactief
- Admin kan toevoegen, aanpassen en verwijderen

---

## 🛠️ Technologie

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQLite
- ASP.NET Identity
- Bootstrap 5

---

## ✅ Vereisten

- .NET SDK 9.0+
- Visual Studio 2022 (of VS Code)

---

## 📁 Projectstructuur

# 🏓 PadelSimple Web

PadelSimple is een **ASP.NET Core MVC webapplicatie** voor het beheren van **padelterreinen, reservaties, materiaal en gebruikers**, met **rolgebaseerde toegang (Admin / User)** en een **SQLite database**.

---

## ✨ Functionaliteiten

### 👤 Gebruikers
- Registreren
- Inloggen / Uitloggen
- Rolgebaseerde toegang:
  - **Admin**: volledig beheer
  - **User**: reservaties bekijken en maken

### 📅 Reservaties
- Terreinen reserveren per **datum & tijdslot**
- Materiaal toevoegen aan reservaties
- Automatisch zien **wanneer een terrein weer vrij is**

### 🏟️ Terreinen
- Indoor / Outdoor
- Capaciteit
- Beschikbaarheid per geselecteerd tijdslot

### 🎒 Materiaal
- Totale en beschikbare hoeveelheid
- Actief / Inactief
- Admin kan toevoegen, aanpassen en verwijderen

---

## 🛠️ Technologie

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQLite
- ASP.NET Identity
- Bootstrap 5

---

## ✅ Vereisten

- .NET SDK 9.0+
- Visual Studio 2022 (of VS Code)

---

## 📁 Projectstructuur

PadelSimpleProject
│
├── PadelSimple.Web → Web applicatie (MVC)
├── PadelSimple.Models → Entities, DbContext, Identity
└── PadelSimple.sln

---

## 🗄️ Database

De applicatie gebruikt **SQLite**.

**appsettings.json**
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=padelsimple.web.db"
  }
}```

🔐 Admin login (User Secrets)

⚠️ Wachtwoorden staan NIET in de code

De admin gebruiker wordt aangemaakt via User Secrets.

1️⃣ Ga naar het webproject
cd .\PadelSimple.Web\

2️⃣ Initialiseer User Secrets
dotnet user-secrets init

3️⃣ Zet admin credentials
dotnet user-secrets set "SeedAdmin:Email" "admin@padel.local"
dotnet user-secrets set "SeedAdmin:Password" "Admin123!"

4️⃣ Controleer
dotnet user-secrets list

🔑 Admin login gegevens
Email	Wachtwoord
admin@padel.local
	Admin123!

📦 Database migraties (optioneel)

Indien nodig:

Add-Migration InitWeb -Project PadelSimple.Web -StartupProject PadelSimple.Web
Update-Database -Project PadelSimple.Web -StartupProject PadelSimple.Web

▶️ Applicatie starten
Via CLI
dotnet run

Via Visual Studio

Zet PadelSimple.Web als Startup Project

Klik op Run

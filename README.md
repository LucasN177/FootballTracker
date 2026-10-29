# ⚽ FootballTracker

Ein kleines **Blazor WebAssembly** Spaß-Projekt rund um Fußball-Daten 📊  

Mit **FootballTracker** kann man Spieltage, Teams und Ergebnisse aus der OpenLiga DB tracken und ein bisschen analysieren.

> ❗ Wichtig: Das hier ist **kein professionelles Produkt**, sondern ein privates Lern- & Freizeitprojekt.

---

## 🚀 Features

- 🏠 **Dashboard** mit Kennzahlen, aktuellem Spieltag, Tabelle und Team-Check
- 🔴 **Live Scores** mit automatischer Aktualisierung
- 🏆 **Wettbewerbe**: 1. Bundesliga, 2. Bundesliga und DFB-Pokal (mit Turnierverlauf)
- 📈 **Tabelle** (gesamt, Heim, Auswärts) inkl. Form, farbigen Tabellenzonen und Tabellenverlauf
- 📅 **Spieltage** mit Filtern, Suche und Spieldetails (Torverlauf, Halbzeit, Zuschauer)
- 👥 **Teams** und **Team-Detailseiten** mit Punkteverlauf, Heim/Auswärts-Bilanz, Torschützen und Spielplan
- 🥅 **Torschützenliste** mit Suche
- ⭐ **Player Tracker**: Lieblingsspieler verfolgen – Tore, Elfmeter, Anteil an Teamtoren, Torverlauf pro Spieltag
- ❤️ **Lieblingsteams** mit letztem und nächstem Spiel
- 📊 **Statistiken**: Torstatistiken, Performance/Formtabelle und Team-Vergleich inkl. direktem Vergleich
- 🌙 Dark Mode, Saisonauswahl, Login/Registrierung und Profil (Supabase)
- 📱 Installierbar als **PWA**

Alles läuft als **Blazor WebAssembly App** direkt im Browser – kein eigenes Backend.

---

## 🌐 Live Demo (Production)

👉 **Hier geht’s zur Live-Version:**  
https://lucasn177.github.io/FootballTracker 


---

## 🛠️ Tech Stack

- ⚡ .NET 9 / Blazor WebAssembly (PWA)
- 🎨 MudBlazor
- 🌐 REST API (OpenLigaDB)
- 🔐 Supabase (Auth & Favoriten)

---

## 🔐 Supabase

Login, Profil und Favoriten nutzen Supabase. Die Zugangsdaten werden beim Deployment aus den
GitHub-Secrets `SUPABASE_URL` und `SUPABASE_KEY` in die `wwwroot/appsettings.json` geschrieben
(siehe `.github/workflows/deploy.yml`).

Das benötigte Datenbankschema (Tabellen `users`, `favorite_players`, `favorite_teams` inkl.
Row Level Security) liegt in [`supabase/schema.sql`](supabase/schema.sql) und kann im Supabase
SQL-Editor ausgeführt werden.

Ohne Zugangsdaten läuft die App trotzdem – Login und Favoriten sind dann deaktiviert.

---

## 📦 Projekt lokal starten

### Voraussetzungen

- .NET 9 SDK

### Repository klonen

```bash
git clone https://github.com/lucasn177/FootballTracker.git
cd FootballTracker
dotnet run
```

Für Login und Favoriten lokal `Supabase:Url` und `Supabase:AnonKey` in `wwwroot/appsettings.json`
eintragen (nicht committen).

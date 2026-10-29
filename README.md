# ⚽ Football Tracker

Ein kleines **Blazor WebAssembly** Spaß-Projekt rund um Fußball-Daten 📊  

Mit **Football Tracker** kann man Tabellen, Spieltage, Teams und Spieler aus der [OpenLigaDB](https://www.openligadb.de)
verfolgen und auswerten – für Bundesliga, 2. Bundesliga, 3. Liga, DFB-Pokal und Champions League.

> ❗ Wichtig: Das hier ist **kein professionelles Produkt**, sondern ein privates Lern- & Freizeitprojekt.

---

## 🚀 Features

### 🏆 Wettbewerbe

| Wettbewerb | Saisons | Besonderheiten | Farbschema |
|---|---|---|---|
| 1. Bundesliga | ab 2020/21 | Tabellenzonen Europapokal, Relegation, Abstieg | Rot |
| 2. Bundesliga | ab 2020/21 | Zonen Aufstieg, Relegation, Abstieg | Blau |
| 3. Liga | ab 2020/21 | 20 Teams, 38 Spieltage, vier Absteiger | Anthrazit |
| DFB-Pokal | ab 2023/24 | Turnierverlauf statt Tabelle, Runden 1. Runde bis Finale | Grün |
| UEFA Champions League | ab 2025/26 | Ligaphase mit eigener Tabelle + K.-o.-Phase (Playoffs bis Finale) | Navy |

Wettbewerb und Saison werden in der App-Leiste gewählt und lokal gespeichert. Jeder Wettbewerb hat ein
eigenes Farbschema und Logo.

### 📋 Seiten

- 🏠 **Dashboard** – Kennzahlen, aktueller Spieltag bzw. Runde, Tabelle, Team-Check (im Pokal: Torjäger, Pokalsieger)
- 🔴 **Live Scores** – laufende Spiele mit automatischer Aktualisierung alle 60 Sekunden
- 📈 **Tabelle** – gesamt, Heim und Auswärts, Form der letzten Spiele, farbige Zonen und **Tabellenverlauf**
  (Platzierung nach jedem Spieltag für bis zu 6 Teams); im Pokal der **Turnierverlauf**, in der Champions League
  zusätzlich die K.-o.-Phase
- 📅 **Spieltage / Runden** – Spieltagsleiste, Filter (Live, Anstehend, Beendet), Teamsuche und Spieldetails
  mit Torverlauf, Halbzeitstand, Verlängerung und Elfmeterschießen
- 👥 **Teams** und **Team-Detailseiten** – Kennzahlen, Tabellen- und Punkteverlauf, Heim/Auswärts-Bilanz,
  Torschützen des Teams und Spielplan (im Pokal: erreichte Runde)
- 🥅 **Torschützen** – Torjägerliste mit Podium und Suche
- ⭐ **Player Tracker** – Lieblingsspieler verfolgen: Tore, Rang, Elfmeter, Spiele mit Tor, Torverlauf pro Spieltag;
  Detailseite je Spieler mit Anteil an den Teamtoren, Torminuten und allen Toren
- ❤️ **Lieblingsteams** – Tabellenplatz, Form, letztes und nächstes Spiel
- 📊 **Statistiken**
  - Torstatistiken: Tore pro Spieltag, Torzeitpunkte, Spielausgänge, Rekorde
  - Performance: Punkteverlauf und Formtabelle
  - Team-Vergleich: zwei Teams gegenübergestellt inkl. direktem Vergleich
- ⚙️ **Einstellungen & Profil** – Dark Mode, Wettbewerb, Saison, Login/Registrierung und Profilbearbeitung

Alles läuft als **Blazor WebAssembly App** direkt im Browser – kein eigenes Backend.
Die App ist als **PWA** installierbar und für Smartphones optimiert.

### ℹ️ Hinweise zu den Daten

- OpenLigaDB liefert keine Kader, Aufstellungen oder Karten. Spieler und ihre Teamzugehörigkeit werden daher
  aus den Torschützen der Spiele abgeleitet – der Player Tracker kennt nur Spieler mit mindestens einem Tor.
- Bei Elfmeterschießen listet OpenLigaDB die verwandelten Elfmeter als Tore; diese werden aus den Statistiken
  herausgerechnet.
- Die Tabelle der Champions League von OpenLigaDB enthält auch K.-o.-Spiele, daher wird die Ligaphasen-Tabelle
  aus den Spieltagen 1–8 berechnet.
- Wettbewerbslogos werden von Wikimedia Commons geladen.

---

## 🌐 Live Demo (Production)

👉 **Hier geht’s zur Live-Version:**  
https://lucasn177.github.io/FootballTracker 


---

## 🛠️ Tech Stack

- ⚡ .NET 9 / Blazor WebAssembly (PWA)
- 🎨 MudBlazor
- 🌐 REST API (OpenLigaDB)
- 🔐 Supabase (Auth, Profil & Favoriten)
- 🚀 GitHub Actions → GitHub Pages

### Projektstruktur

```
Core/            Modelle (Spiele, Teams, Wettbewerbe, Statistiken) und Interfaces
Infrastructure/  OpenLigaDB- und Supabase-Repositories, Session-Speicherung
Services/        Spieldaten, Tabellen, Statistiken, Favoriten, Auth, App-Zustand
Pages/           Seiten, Komponenten und Dialoge
Layout/          Hauptlayout und Theme je Wettbewerb
supabase/        Datenbankschema
```

---

## 🔐 Supabase

Login, Profil und Favoriten (Lieblingsspieler und -teams) nutzen Supabase. Die Zugangsdaten werden beim Deployment
aus den GitHub-Secrets `SUPABASE_URL` und `SUPABASE_KEY` in die `wwwroot/appsettings.json` geschrieben
(siehe `.github/workflows/deploy.yml`).

Das benötigte Datenbankschema (Tabellen `users`, `favorite_players`, `favorite_teams` inkl.
Row Level Security und Trigger für neue Nutzer) liegt in [`supabase/schema.sql`](supabase/schema.sql) und kann
im Supabase SQL-Editor ausgeführt werden.

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

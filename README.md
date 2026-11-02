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
  Torschützen des Teams und Spielplan (im Pokal: erreichte Runde). Dazu ein Monatskalender mit allen Spielen der
  Saison über alle Wettbewerbe – Liga, DFB-Pokal und Champions League – farblich nach Wettbewerb und filterbar
- 🥅 **Torschützen** – Torjägerliste mit Podium und Suche
- ⭐ **Player Tracker** – Lieblingsspieler verfolgen: Tore, Rang, Elfmeter, Spiele mit Tor, Torverlauf pro Spieltag;
  Detailseite je Spieler mit Anteil an den Teamtoren, Torminuten und allen Toren
- ❤️ **Lieblingsteams** – Tabellenplatz, Form, letztes und nächstes Spiel
- 📊 **Statistiken**
  - Torstatistiken: Tore pro Spieltag, Torzeitpunkte, Spielausgänge, Rekorde
  - Performance: Punkteverlauf und Formtabelle
  - Team-Vergleich: zwei Teams gegenübergestellt inkl. direktem Vergleich
  - Prognosen: Tipp, erwartete Tore und Sieg/Remis/Niederlage-Wahrscheinlichkeiten je Spiel, Teamstärken und
    eine Saisonbilanz des Modells (Tendenz, Sieger/Remis, exakte Treffer, Kicktipp-Punkte, Brier-Score).
    Tipp-Strategie wählbar: Kicktipp-optimal oder mit Remis-Tipps.
    - Prognose im Detail (Klick auf eine Spielkarte): Teamstärken mit den zugrunde liegenden Spielen und ihrem Gewicht,
      Rechnung der erwarteten Tore, Torverteilung, Ergebnis-Matrix mit Dixon-Coles-Korrektur, Sieg/Remis/Niederlage,
      erwartete Kicktipp-Punkte möglicher Tipps und – nach Spielende – ein Rückblick
    - Saisonprognose: 10.000 simulierte Saisonverläufe ergeben die Chancen auf Meisterschaft, Europapokal,
      Aufstieg, Relegation und Abstieg (Champions League: Top 8, Playoffs, Aus)
    - Verlauf: Trefferquote je Spieltag und Bilanz des Modells in allen Saisons
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

### 🔮 Prognosemodell

Die Prognosen nutzen ein **Dixon-Coles-Modell**: Jedes Team hat einen Angriffs- und einen Abwehrwert, dazu kommen
ein ligaweiter Heimvorteil und ein Korrekturfaktor ρ für knappe Ergebnisse (0:0, 1:0, 0:1, 1:1). Die erwarteten Tore
beider Teams ergeben über die Poisson-Verteilung die Wahrscheinlichkeit jedes Ergebnisses.

- Die Werte werden per gewichteter Maximum-Likelihood aus der laufenden Saison und der Vorsaison geschätzt. Dadurch
  fließt auch die Stärke der bisherigen Gegner ein.
- Ein Spiel verliert nach 270 Tagen die Hälfte seines Gewichts. Teamwerte werden zum Ligaschnitt gezogen, Aufsteiger
  starten etwas schwächer.
- Der Tipp ist das Ergebnis mit den meisten erwarteten Kicktipp-Punkten (4 exakt, 3 Tordifferenz, 2 Tendenz).
  Dabei kommt fast nie ein Remis heraus. Die Strategie „Mit Remis-Tipps“ tippt das wahrscheinlichste Remis, wenn
  Heim- und Auswärtssieg weniger als 10 Prozentpunkte auseinanderliegen – im Backtest etwa so oft wie tatsächlich
  Remis fallen, für rund 0,07 Kicktipp-Punkte weniger pro Spiel.
- Die Saisonprognose spielt die offenen Spiele 10.000-mal nach den Ergebniswahrscheinlichkeiten durch. Die Teamstärken
  bleiben dabei fest; bei Gleichstand entscheiden Tordifferenz, Tore und danach das Los.
- Für jeden Spieltag wird nur mit Spielen gerechnet, die vorher beendet waren. So ist die Saisonbilanz ein ehrlicher
  Rückblick.

Halbwertszeit und Regularisierung wurden per Backtest über 1. und 2. Bundesliga 2021/22 bis 2025/26 (3.060 Spiele)
gewählt: rund 49 % richtige Tendenzen (immer Heimsieg: 44 %), 9 % exakte Ergebnisse, Brier-Score 0,614.

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
Services/        Spieldaten, Tabellen, Statistiken, Prognosen, Favoriten, Auth, App-Zustand
Pages/           Seiten, Komponenten und Dialoge
Layout/          Hauptlayout und Theme je Wettbewerb
supabase/        Datenbankschema
Tests/           xUnit-Tests (Prognosemodell, Kicktipp-Wertung, Saisonsimulation, Tabellenberechnung)
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

### Tests ausführen

```bash
dotnet test Tests/FootballTracker.Tests.csproj
```

Die Tests laufen auch im Deployment-Workflow vor dem Veröffentlichen.

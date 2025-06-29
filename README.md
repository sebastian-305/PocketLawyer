# PocketLawyer

PocketLawyer ist eine Webanwendung auf Basis von **ASP.NET 8**, welche Arbeitsverträge analysiert. Die Anwendung kann PDF- und Bilddateien über Tesseract OCR auslesen, sendet die extrahierten Texte an verschiedene KI-Dienste und stellt die Ergebnisse übersichtlich dar.

## Funktionsweise
* **Gemini API**: Hauptdienst für die Analyse von Verträgen.
* **Claude API**: Optionaler Dienst, der aktuell für die Erkennung des passenden Kollektivvertrags genutzt wird.
* **Tesseract OCR**: Unterstützt das Einlesen von PDFs oder Bildern und wandelt sie in Text um.

Eine Datei wird zunächst vorvalidiert. Ist sie lesbar und als Arbeitsvertrag erkennbar, wird eine tiefere Analyse durchgeführt. Die Analyse bewertet Klauseln, gibt Zusammenfassungen und stellt Verstöße gegen Gesetz und Kollektivvertrag heraus.

## Ordnerstruktur

```
Dtos/            Enthält die Datenobjekte (DTOs) für Requests und Responses.
Infrastructure/  Implementierung der Dienste (Gemini, Claude, Dateidecoder, KV-Erkennung etc.).
Service/         Schnittstellen und gemeinsame Service-Definitionen.
wwwroot/         Statische Dateien der Weboberfläche (HTML, JS, CSS, Medien).
Responses/       Ablage für gesicherte Antworten der KI.
Samples/         Beispielverträge für Tests.
Diagrams/        Architektur- und Ablaufdiagramme.
tessdata/        Sprachdaten für Tesseract OCR.
```

## Endpunkte

* `GET  /analysis` – WebSocket-Endpunkt, der nach dem Senden einer Datei zuerst die Voranalyse und danach die Hauptanalyse überträgt.
* `GET  /mockdata` – Liefert zufällige Beispielergebnisse (JSON) aus dem `Responses`-Ordner.

## Installation und Ausführung

1. Abhängigkeiten wiederherstellen:
   ```bash
   dotnet restore
   ```
2. Anwendung starten:
   ```bash
   dotnet run
   ```
3. Die Weboberfläche ist danach unter `http://localhost:5112` erreichbar (vgl. `launchSettings.json`).

## Konfiguration
Benötigte Einstellungen befinden sich in `appsettings.json` bzw. den User-Secrets:

```
Gemini:Key            = API-Schlüssel für Gemini
Gemini:Key:Alternative = optionaler Ausweichschlüssel
Gemini:ModelId        = Modell-ID (standardmäßig "gemini-1.5-flash-002")
Claude:Key            = API-Schlüssel für Claude (optional)
TesseractDataPath     = Pfad zu den OCR-Sprachdaten
ResponsesPath         = Ablageort für AI-Antworten
MaxUploadSizeMB       = Maximale Upload-Größe
KVSPath               = Pfad zu den Kollektivvertragsdateien
```

API-Schlüssel sollten über Umgebungsvariablen oder User-Secrets gesetzt werden und **nicht** im Quellcode liegen.

## Lizenz & Beitrag
Dieses Projekt steht ohne konkrete Lizenzangabe bereit. Beiträge sind willkommen. Weitere Informationen und Diagramme befinden sich im Ordner `Diagrams`.


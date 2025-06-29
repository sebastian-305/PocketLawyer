# Design Patterns Übersicht

Dieses Dokument identifiziert die im Projekt PocketLawyer verwendeten Design- und Architekturprinzipien. In jedem Abschnitt wird aufgeführt, wo das jeweilige Muster im Code eingesetzt wird und weshalb es nützlich ist.

## Software-Design-Prinzipien

### Dependency Injection
* **Implementierung:** Die Dependency Injection wird in `Program.cs` mithilfe von `builder.Services` konfiguriert. So werden Dienste und keyed-Implementierungen registriert. Beispielhafte Registrierungen:
  ```csharp
  builder.Services.AddKeyedScoped<IAiService, GeminiService>("gemini");
  builder.Services.AddKeyedScoped<IAiService, ClaudeService>("claude");
  builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
  ```
  Diese Registrierungen ermöglichen es Controllern und Services, sich auf Abstraktionen (`IAiService`, `IFileDecoderService` usw.) zu stützen, anstatt auf konkrete Klassen.
* **Nutzen:** Fördert lose Kopplung und erfüllt das Dependency-Inversion-Prinzip, da Implementierungen bei Bedarf ausgetauscht oder gemockt werden können.

### SOLID & DRY
* **Implementierung:** Das Projekt definiert Schnittstellen (`IAiService`, `IFileDecoder`, `IKvDetectionService` usw.) mit klar abgegrenzten Verantwortlichkeiten. Die Serviceklassen implementieren diese Schnittstellen. Diese Trennung folgt dem Single-Responsibility- und Interface-Segregation-Prinzip. Validierung und weitere Verarbeitung übernimmt nun der `WebSocketAnalysisService` zusammen mit spezialisierten Services anstelle von Controller-Hilfsmethoden.
* **Nutzen:** Sauber definierte Schnittstellen vermeiden enge Kopplung und verhindern Wiederholungen bei Dekodierungs- oder Validierungslogik, womit das DRY-Prinzip eingehalten wird.

### GRASP-Prinzipien
* **Information Expert & Controller:** Der `WebSocketAnalysisService` koordiniert die Dekodierung, die KV-Erkennung und die KI-Aufrufe. Spezialisierte Aufgaben werden an andere Services delegiert, was den Controller- und Information-Expert-Rollen entspricht.
* **Low Coupling:** Die Services hängen lediglich von Abstraktionen ab. `PocketController` greift in seinen Operationen nur auf Schnittstellen zurück.

### YAGNI
* **Beobachtung:** Der Code implementiert nur die aktuell benötigten Funktionen (z. B. eine begrenzte Zahl von Dateidekodierern). Nicht benötigte Generalisierungen fehlen, was dem "You Aren't Gonna Need It"-Prinzip entspricht.

## Domain Driven Design

### Anemisches Domain Model
* **Nachweis:** Die DTO-Klassen in `Dtos/` enthalten Daten ohne eigenes Verhalten, z. B. `ContractAnalysisDto` und `ContractValidationDto`. Die Domain-Logik liegt überwiegend in den Services, nicht in den DTOs selbst.
* **Auswirkung:** Dies vereinfacht Serialisierung und Transport, konzentriert die Geschäftslogik jedoch in den Serviceklassen.

### Value Objects
* **Nachweis:** `FileContentDto` ist ein unveränderliches Record, das einen Dateinamen und dessen Inhalt speichert.
* **Nutzen:** Kleine Datenstrukturen als Value Objects zu behandeln erhöht die Klarheit beim Austausch zwischen den Schichten.

### Smart UI
* **Beobachtung:** Das JavaScript-Frontend in `wwwroot` übernimmt das Rendering und kommuniziert über WebSockets mit dem Server. Auf dem Server werden aufwendige Analysen an die Backend-Services delegiert, sodass die Oberfläche "smart" bleibt, aber schlank.

## Strukturelle Muster

### Fassade
* **Implementierung:** `AnalyzeContractUseCase` fungiert als Fassade, die die Komplexität von Dateidekodierung, Kollektivvertragserkennung und KI-Aufrufen verbirgt. Verbraucher rufen lediglich `ProcessContractRequest` auf, ohne sich um die zugrunde liegenden Schritte zu kümmern.
* **Nutzen:** Vereinfacht die Logik im Controller und stellt einen einzigen Einstiegspunkt für die Vertragsanalyse bereit. Dadurch kann der Controller schlank bleiben und wiederverwendbarer Code entsteht.
* **Alternative:** Der Controller könnte jeden Schritt selbst orchestrieren. Das würde jedoch die Wiederverwendbarkeit einschränken und den Code schwerer testbar machen.

### Adapter
* **Implementierung:** Die Klassen der Dekodierungsstrategie (`TextFileDecoder`, `PdfFileDecoder`, `ImageFileDecoder`) passen verschiedene Bibliotheken (Plain-Text-Leser, PdfPig, Tesseract) an das gemeinsame Interface `IFileDecoder` an.
* **Nutzen:** Dadurch kann der `FileDecoderService` unterschiedliche Dekodierungsmechanismen einheitlich behandeln und einfach erweitern.
* **Alternative:** Man hätte die Bibliotheken direkt im `FileDecoderService` ansprechen können. Das hätte aber zu dupliziertem Code und enger Kopplung geführt.


## Verhaltensmuster

### Strategy
* **Implementierung:** `FileDecoderService` wählt zur Laufzeit eine Dekodierungsstrategie aus den registrierten `IFileDecoder`-Implementierungen auf Basis der Dateierweiterung.
```csharp
var decoder = _decoders.FirstOrDefault(d => d.CanDecode(extension));
if (decoder == null)
    throw new NotSupportedException("Unknown file extension: " + extension);
return await decoder.DecodeAsync(content);
```
* **Nutzen:** Es lässt sich leicht Unterstützung für neue Dateitypen ergänzen, indem eine weitere `IFileDecoder`-Implementierung bereitgestellt wird, ohne bestehenden Code anzupassen. Die Implementierungen bleiben voneinander getrennt.
* **Alternative:** Ein `switch`-Block im Service könnte jede Erweiterung einzeln behandeln. Das würde jedoch schnell unübersichtlich werden und bei neuen Dateitypen Änderungen am bestehenden Code erfordern.

## Zusammenfassung
PocketLawyer setzt stark auf Dependency Injection und ein schnittstellenbasiertes Design und orientiert sich an den SOLID- sowie GRASP-Prinzipien. Das Domain Model ist überwiegend anämisch, einfache DTOs repräsentieren die Daten. Strukturelle Muster wie Fassade und Adapter vereinfachen die Interaktion der Subsysteme, während das Strategy-Muster beim Dateidekodieren zum Einsatz kommt.

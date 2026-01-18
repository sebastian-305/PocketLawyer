# Legacy-Dokumentation und -Tools

Dieser Ordner enthält veraltete Dokumentation und Tools, die nicht mehr aktiv verwendet werden.

## Dokumentationsgenerator (VERALTET)

Der automatische Dokumentationsgenerator wurde eingestellt, da er zu aufwändig in der Wartung war.

### Verschobene Dateien:

**Generator-Code:**
- `Documentation/` - Kompletter Dokumentationsgenerator mit Extraction, Generation und Verification
  - `Documentation/Extraction/` - Code-Analyse-Tools
  - `Documentation/Generation/` - Dokumentationsgeneratoren
  - `Documentation/Verification/` - Verifikations-Tools

**Generator-Skripte:**
- `generate_documentation.csx` - Hauptskript zur Dokumentationsgenerierung
- `run_documentation_generation.ps1` - PowerShell-Wrapper
- `test_config_analyzer.csx` - Test-Skript für Konfigurationsanalyse
- `test_readme_generator.csx` - Test-Skript für README-Generierung

**Planungs- und Verifikationsdokumente:**
- `DOCUMENTATION_PLAN.md` - Ursprünglicher Dokumentationsplan
- `IMPLEMENTATION_SUMMARY.md` - Implementierungszusammenfassung
- `TASK_3.2_VERIFICATION.md` - Verifikation Task 3.2
- `TASK_3.3_VERIFICATION.md` - Verifikation Task 3.3
- `TASK_3.4_VERIFICATION.md` - Verifikation Task 3.4
- `TASK_3.5_VERIFICATION.md` - Verifikation Task 3.5
- `TASK_4_CHECKPOINT_SUMMARY.md` - Checkpoint-Zusammenfassung Task 4

## Aktuelle Dokumentation

Die aktuelle, manuell gepflegte Dokumentation befindet sich in:

- `docs/README.md` - Hauptdokumentation
- `docs/ARCHITECTURE.md` - Systemarchitektur
- `docs/API_REFERENCE.md` - API-Referenz
- `docs/DATA_MODELS.md` - Datenmodelle
- `docs/CONFIGURATION.md` - Konfiguration
- `docs/DEPLOYMENT.md` - Deployment-Anleitung

## Hinweis

Diese Dateien werden nur zu Referenzzwecken aufbewahrt und sollten nicht mehr verwendet werden. Die Dokumentation wird jetzt manuell gepflegt.

## Betroffene Tests

Die folgenden Test-Dateien verwenden den veralteten Dokumentationsgenerator und sollten ebenfalls als veraltet betrachtet werden:

- `PocketLawyer.Tests/Documentation/Generation/` - Alle Generator-Tests
- `PocketLawyer.Tests/Documentation/Verification/` - Alle Verifikations-Tests
- `PocketLawyer.Tests/Documentation/Extraction/` - Alle Extraktions-Tests

Diese Tests können bei Bedarf entfernt oder deaktiviert werden, da der Generator nicht mehr verwendet wird.

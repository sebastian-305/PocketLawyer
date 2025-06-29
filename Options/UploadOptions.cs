namespace PocketLawyer.Options;

// Konfiguration der Uploadgröße für Dateien.

// Enthält Einstellungen für Dateiuploads.
public class UploadOptions
{
    public long MaxUploadSizeMB { get; set; } = 20;
}

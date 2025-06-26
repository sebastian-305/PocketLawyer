namespace PocketLawyer.Service
{
    // Diese Schnittstelle kapselt das Einlesen verschiedener Dateiformate.
    // Dadurch kann die Controllerlogik unabhängig vom tatsächlichen
    // Dateiformat bleiben.
    public interface IFileDecoderService
    {
        // Liste der erlaubten Dateiendungen.
        public string[] AllowedExtensions { get; }
        // Gleiches als kommaseparierter String für Fehlermeldungen.
        public string AllowedExtensionsString { get; }
        // Wandelt eine hochgeladene Datei in reinen Text um.
        public Task<string> GetFileContentAsync(string filename, Stream content);
    }
}

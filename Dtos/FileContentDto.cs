namespace PocketLawyer;

// Hilfsrecord, um den entschlüsselten Dateinamen und den Textinhalt
// zwischen Methoden weiterzureichen.

public record FileContentDto(string FileName, string FileContent);

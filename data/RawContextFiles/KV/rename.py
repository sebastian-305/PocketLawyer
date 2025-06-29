#!/usr/bin/env python3
"""
Tool zum maschinenlesbaren Umbenennen von Ordnern im aktuellen Verzeichnis.

Vorgehen:
1. Listet alle Unterordner im aktuellen Arbeitsverzeichnis auf.
2. Wandelt jeden Ordnernamen in eine "slug"-Form um:
   - Unicode-Normalisierung
   - Deutsche Umlaute (Ä,Ö,Ü,ä,ö,ü) -> AE,OE,UE / ae,oe,ue
   - ß -> ss
   - Entfernen aller Zeichen außer Buchstaben, Ziffern, Leerzeichen, Bindestrich und Unterstrich
   - Ersetzen von Leerzeichen und Bindestrichen durch Unterstriche
   - Umwandeln in Kleinbuchstaben
   - Entfernen führender/abschlussender Unterstriche
3. Zeigt eine Vorschau aller Umbenennungen an und fragt zur Bestätigung.
4. Führt die Umbenennung durch, sofern bestätigt.

Annahmen:
- Das Skript wird im Verzeichnis mit den umzubenennenden Ordnern ausgeführt.
- Es existieren keine gleichgültigen Namenskollisionen nach der Slug-Erzeugung;
  bei Kollision wird der jeweilige Ordner übersprungen und mitgeteilt.
- Nur Ordner werden umbenannt, Dateien bleiben unberührt.

Mögliche Unzulänglichkeiten:
- Kollidierende Slugs: Zwei verschiedene Originalnamen könnten auf denselben Slug treffen.
- Unvorhergesehene Sonderzeichen außerhalb des deutschen Alphabets könnten entfernt werden.
- Bei sehr langen Namen kann der Zielname unübersichtlich werden.
- Keine Rückgängig-Funktion: Bei Bedarf vorher Backup oder Versionskontrolle nutzen.
"""
import os
import re
import unicodedata


def slugify(name: str) -> str:
    """Erzeugt einen maschinenlesbaren Slug aus einem gegebenen Namen."""
    # Unicode normalisieren
    normalized = unicodedata.normalize('NFKD', name)
    # Deutsche Umlaute und ß ersetzen
    trans = {
        'Ä':'Ae','Ö':'Oe','Ü':'Ue',
        'ä':'ae','ö':'oe','ü':'ue','ß':'ss'
    }
    for orig, repl in trans.items():
        normalized = normalized.replace(orig, repl)
    # Alle unerwünschten Zeichen entfernen
    cleaned = re.sub(r'[^0-9A-Za-z _-]', '', normalized)
    # Leerzeichen und Bindestriche durch Unterstriche ersetzen
    underscored = re.sub(r'[ \-]+', '_', cleaned)
    # Kleinbuchstaben und führende/trailing Unterstriche trimmen
    slug = underscored.strip('_').lower()
    return slug


def main():
    cwd = os.getcwd()
    # Nur Verzeichnisse ermitteln
    entries = os.listdir(cwd)
    folders = [d for d in entries if os.path.isdir(os.path.join(cwd, d))]

    # Mapping alter -> neuer Name
    mapping: dict[str, str] = {}
    seen: set[str] = set()
    for folder in folders:
        new_name = slugify(folder)
        if new_name and new_name != folder:
            if new_name in seen:
                print(f"⚠ Kollision erkannt: '{folder}' -> '{new_name}' (bereits vergeben)")
                continue
            mapping[folder] = new_name
            seen.add(new_name)

    if not mapping:
        print("Keine zu ändernden Ordnernamen gefunden.")
        return

    # Vorschau
    print("Vorschau der Umbenennungen:")
    for old, new in mapping.items():
        print(f"  {old}  ->  {new}")

    ans = input("Möchtest du diese Umbenennungen durchführen? (j/N): ")
    if ans.strip().lower() != 'j':
        print("Abgebrochen.")
        return

    # Durchführung
    for old, new in mapping.items():
        try:
            os.rename(old, new)
            print(f"✔ '{old}' umbenannt in '{new}'")
        except Exception as e:
            print(f"✘ Fehler beim Umbenennen von '{old}': {e}")

    print("Umbenennung abgeschlossen.")


if __name__ == '__main__':
    main()

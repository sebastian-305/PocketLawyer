#!/usr/bin/env python3
"""
Tool zum Herunterladen aller Kollektivverträge und ihrer Anhänge von der Vida-Website.

Vorgehen:
1. Verwende eine fest definierte Liste von Kategorie-Slugs, da die Verträge nicht direkt auf der Basis-URL liegen.
2. Für jede Kategorie-Seite alle Überschriften (h3) als Vertragsnamen nutzen und in jeweiligen Ordnern speichern.
3. Unter jedem Vertrag alle .pdf-Links herunterladen.

Annahmen:
- Die Pfade zur Kategorie-Seite entsprechen <BASE_URL>/<slug> mit Slugs:
  dienstleistungen, eisenbahn, gebaeudemanagement, gesundheit,
  luft-und-schiffverkehr, soziale-dienste, strasse, tourismus
- Jede Vertragsüberschrift ist in einem <h3>-Tag.
- Anhänge und Verträge sind immer .pdf und enden auf '.pdf'.
- Die Seitenstruktur ändert sich nicht wesentlich.
"""
import requests
from bs4 import BeautifulSoup
import os
import urllib.parse
import re

BASE_URL = "https://www.vida.at/de/wir-helfen/dein-kollektivvertrag"
CATEGORY_SLUGS = [
    "dienstleistungen",
    "eisenbahn",
    "gebaeudemanagement",
    "gesundheit",
    "luft-und-schiffverkehr",
    "soziale-dienste",
    "strasse",
    "tourismus",
]


def sanitize_filename(name: str) -> str:
    """Ersetzt unzulässige Zeichen in Dateinamen."""
    return re.sub(r'[\\/:*?"<>|]', '_', name).strip()


def download_file(url: str, dest_folder: str):
    """Lädt eine Datei herunter und speichert sie im Zielordner."""
    parsed = urllib.parse.urlparse(url)
    filename = sanitize_filename(os.path.basename(parsed.path))
    dest_path = os.path.join(dest_folder, filename)
    try:
        with requests.get(url, stream=True) as response:
            response.raise_for_status()
            with open(dest_path, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    f.write(chunk)
        print(f"✔ {filename} heruntergeladen nach {dest_folder}")
    except Exception as e:
        print(f"✘ Fehler beim Herunterladen von {url}: {e}")


def process_category(slug: str):
    """Verarbeitet eine Kategorie-Seite und lädt alle PDFs unter jedem Vertrag (h3) herunter."""
    url = f"{BASE_URL}/{slug}"
    resp = requests.get(url)
    resp.raise_for_status()
    soup = BeautifulSoup(resp.text, 'html.parser')

    for header in soup.find_all('h3'):
        title = header.get_text(strip=True)
        folder = sanitize_filename(title)
        os.makedirs(folder, exist_ok=True)
        for sib in header.find_next_siblings():
            if sib.name == 'h3':
                break
            for a in sib.find_all('a', href=True):
                href = a['href']
                if href.lower().endswith('.pdf'):
                    pdf_url = urllib.parse.urljoin(url, href)
                    download_file(pdf_url, folder)


def main():
    for slug in CATEGORY_SLUGS:
        print(f"==> Verarbeite Kategorie: {slug}")
        process_category(slug)


if __name__ == '__main__':
    main()

import { startAnalysisSocket, requestMockAnalysis, mockPreValidationMessage } from './network.js';
import { createLoadingElement, renderPreValidation, renderContractResult, displayUploadError, printer } from './render.js';

// Dieses Skript steuert den Dateiupload und die Darstellung der Analyse.

const DEBUG = false;

// Wird beim Abschicken des Upload-Formulars aufgerufen.
async function uploadFile(event) {
    event.preventDefault(); // Verhindert das eigentliche Absenden des Formulars

    if (DEBUG) console.debug('File upload started.');

    // Bereich des Modals vorbereiten und Ladeindikator einfügen
    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    // Eingabefeld der Datei ermitteln
    const input = event.target.myfile;
    let preAnalysisShown = false;
    let preMessage = null;

    try {
        // Kein File gewählt? -> Demomodus mit Mock-Daten
        if (input.files.length === 0) {
            // Sofort eine statische Voranalyse anzeigen
            preMessage = mockPreValidationMessage();
            body.replaceChildren();
            renderPreValidation(preMessage, body);
            preAnalysisShown = true;
            // Ladeindikator während wir die Beispielanalyse laden
            body.appendChild(loading);
            const analysisMessage = await requestMockAnalysis();
            body.removeChild(loading);
            renderContractResult(analysisMessage, body);
            // Ergebnis im SessionStorage sichern, damit es wiederhergestellt werden kann
            sessionStorage.setItem(
                'analysisResult',
                JSON.stringify({ validation: preMessage, analysis: analysisMessage }),
            );
            document.getElementById('printbtn').style.visibility = 'visible';
        } else {
            // Datei vorhanden -> echten Upload via WebSocket starten
            const file = input.files[0];
            const socket = startAnalysisSocket(file, (msg) => {
                if (!preAnalysisShown) {
                    preMessage = msg;
                    body.replaceChildren();
                    renderPreValidation(msg, body);
                    preAnalysisShown = true;
                    if (!msg.isReadable || !msg.isContract) {
                        socket.close();
                    } else {
                        body.appendChild(loading);
                    }
                } else {
                    body.removeChild(loading);
                    renderContractResult(msg, body);
                    // Analyse und Voranalyse zur späteren Wiederherstellung speichern
                    sessionStorage.setItem(
                        'analysisResult',
                        JSON.stringify({ validation: preMessage, analysis: msg }),
                    );
                    document.getElementById('printbtn').style.visibility = 'visible';
                    socket.close();
                }
            });
            socket.addEventListener('error', () => {
                displayUploadError(new Error('WebSocket Fehler'), body, loading, preAnalysisShown);
            });
        }
    } catch (ex) {
        // Fehler behandeln und dem Nutzer anzeigen
        displayUploadError(ex, body, loading, preAnalysisShown);
    } finally {
        // Dateieingabefeld zurücksetzen
        input.value = null;
    }
}

// Initialisiert die Buttons und stellt ggf. gespeicherte Ergebnisse wieder her,
// sobald das Dokument komplett geladen ist.
document.addEventListener('DOMContentLoaded', () => {
    document
        .querySelectorAll('.upload-form')
        .forEach((form) => form.addEventListener('submit', uploadFile));
    window.printer = printer;

    // Toast zum Wiederherstellen früherer Ergebnisse
    const restoreToastEl = document.getElementById('restoreToast');
    const restoreToast = bootstrap.Toast.getOrCreateInstance(restoreToastEl, { autohide: false });
    const evaluationModalEl = document.getElementById('evaluation');

    // Wenn das Analysefenster geschlossen wird und Daten vorliegen, Zeige einen Hinweis
    evaluationModalEl.addEventListener('hidden.bs.modal', () => {
        if (sessionStorage.getItem('analysisResult')) {
            restoreToast.show();
        }
    });

    // Beim Öffnen des Modals den Hinweis ausblenden
    evaluationModalEl.addEventListener('show.bs.modal', () => {
        restoreToast.hide();
    });

    document
        .getElementById('restoreBtn')
        // Klick auf "Wiederherstellen" lädt die letzte Analyse aus dem Speicher
        .addEventListener('click', () => {
            const stored = sessionStorage.getItem('analysisResult');
            if (stored) {
                const { validation, analysis } = JSON.parse(stored);
                const body = document.getElementById('evaluation-body');
                body.replaceChildren();
                renderPreValidation(validation, body);
                renderContractResult(analysis, body);
                document.getElementById('printbtn').style.visibility = 'visible';
                const modal = bootstrap.Modal.getOrCreateInstance(
                    evaluationModalEl,
                );
                modal.show();
            }
        });
});

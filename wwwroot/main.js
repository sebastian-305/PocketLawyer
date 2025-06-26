import { prepareRequests, requestPreValidation, requestContractAnalysis } from './network.js';
import { createLoadingElement, renderPreValidation, renderContractResult, displayUploadError, printer } from './render.js';

// Wird beim Absenden des Upload-Formulars aufgerufen
async function upload_file(event) {
    event.preventDefault();

    console.debug('File upload started.');

    // Ergebnisbereich im Modal
    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    const { validationRequest, analyzeRequest, mock } = prepareRequests(event.target.myfile);

    let preAnalysisShown = false;

    try {
        // Schritt 1: Voranalyse beim Server anfragen
        const validationMessage = await requestPreValidation(validationRequest, mock);
        renderPreValidation(validationMessage, body);
        preAnalysisShown = true;

        if (!validationMessage.isReadable || !validationMessage.isContract) {
            throw new Error(`isReadable: ${validationMessage.isReadable}, isContract: ${validationMessage.isContract}`);
        }

        // Schritt 2: eigentliche Analyse anfordern
        body.appendChild(loading);
        const analysisMessage = await requestContractAnalysis(analyzeRequest);
        body.removeChild(loading);
        renderContractResult(analysisMessage, body);
        // Ergebnisse für eine spätere Wiederherstellung zwischenspeichern
        sessionStorage.setItem(
            'analysisResult',
            JSON.stringify({
                validation: validationMessage,
                analysis: analysisMessage,
            }),
        );
        document.getElementById('printbtn').style.visibility = 'visible';
    } catch (ex) {
        // Fehleranzeige im Modal
        displayUploadError(ex, body, loading, preAnalysisShown);
    } finally {
        event.target.myfile.value = null;
    }
}

// Initialisierung nach dem Laden der Seite
document.addEventListener('DOMContentLoaded', () => {
    document
        .querySelectorAll('.upload-form')
        .forEach((form) => form.addEventListener('submit', upload_file));
    // Funktion zum Drucken global verfügbar machen
    window.printer = printer;

    // Toast zum Wiederherstellen der letzten Analyse
    const restoreToastEl = document.getElementById('restoreToast');
    const restoreToast = bootstrap.Toast.getOrCreateInstance(restoreToastEl);
    const evaluationModalEl = document.getElementById('evaluation');

    // Wenn das Modal geschlossen wird, bieten wir ggf. eine Wiederherstellung an
    evaluationModalEl.addEventListener('hidden.bs.modal', () => {
        if (sessionStorage.getItem('analysisResult')) {
            restoreToast.show();
        }
    });

    evaluationModalEl.addEventListener('show.bs.modal', () => {
        restoreToast.hide();
    });

    // Klick auf "Analyse wiederherstellen"
    document
        .getElementById('restoreBtn')
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

// Einstiegspunkt für die Browserlogik. Kümmert sich um das Upload-Formular und
// koordiniert die Aufrufe an das Backend.
import { prepareRequests, createAnalysisRequest, requestPreValidation, requestContractAnalysis } from './network.js';
import { createLoadingElement, renderPreValidation, renderContractResult, displayUploadError, printer } from './render.js';

// Behandelt das Absenden des Upload-Formulars
async function uploadFile(event) {
    event.preventDefault();

    console.debug('File upload started.');

    // Ersetzt den Inhalt des Modals durch einen Ladeindikator
    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    // Erstellt die Request-Objekte je nachdem, ob eine Datei ausgewählt wurde
    const { validationRequest, file, mock } = prepareRequests(event.target.myfile);

    let preAnalysisShown = false;

    try {
        // Erster Request: Lesbarkeit und Typ prüfen
        const validationMessage = await requestPreValidation(validationRequest, mock);
        renderPreValidation(validationMessage, body);
        preAnalysisShown = true;

        if (!validationMessage.isReadable || !validationMessage.isContract) {
            throw new Error(`isReadable: ${validationMessage.isReadable}, isContract: ${validationMessage.isContract}`);
        }

        // Zweiter Request nur, wenn Schritt 1 erfolgreich war
        const analysisRequest = createAnalysisRequest(
            file,
            validationMessage.kollektivvertrag,
            mock,
        );
        body.appendChild(loading);
        const analysisMessage = await requestContractAnalysis(analysisRequest);
        body.removeChild(loading);
        renderContractResult(analysisMessage, body);
        // Ergebnis speichern, damit es später ohne erneutes Hochladen
        // wiederhergestellt werden kann
        sessionStorage.setItem(
            'analysisResult',
            JSON.stringify({
                validation: validationMessage,
                analysis: analysisMessage,
            }),
        );
        document.getElementById('printbtn').style.visibility = 'visible';
    } catch (ex) {
        displayUploadError(ex, body, loading, preAnalysisShown);
    } finally {
        event.target.myfile.value = null;
    }
}

// Event-Handler einrichten, sobald die Seite geladen ist
document.addEventListener('DOMContentLoaded', () => {
    document
        .querySelectorAll('.upload-form')
        .forEach((form) => form.addEventListener('submit', uploadFile));
    window.printer = printer;

    const restoreToastEl = document.getElementById('restoreToast');
    const restoreToast = bootstrap.Toast.getOrCreateInstance(restoreToastEl, { autohide: false });
    const evaluationModalEl = document.getElementById('evaluation');

    // Zeigt einen Toast zum Wiederherstellen an, wenn das Modal geschlossen wird
    evaluationModalEl.addEventListener('hidden.bs.modal', () => {
        if (sessionStorage.getItem('analysisResult')) {
            restoreToast.show();
        }
    });

    evaluationModalEl.addEventListener('show.bs.modal', () => {
        restoreToast.hide();
    });

    // Stellt zuvor analysierte Ergebnisse aus dem sessionStorage wieder her
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

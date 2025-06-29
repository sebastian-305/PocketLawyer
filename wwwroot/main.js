import { startAnalysisSocket, requestMockAnalysis, mockPreValidationMessage } from './network.js';
import { createLoadingElement, renderPreValidation, renderContractResult, displayUploadError, printer } from './render.js';

// Dieses Skript steuert den Dateiupload und die Darstellung der Analyse.

const DEBUG = false;

// Wird beim Abschicken des Upload-Formulars aufgerufen.
async function uploadFile(event) {
    event.preventDefault();

    if (DEBUG) console.debug('File upload started.');

    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    const input = event.target.myfile;
    let preAnalysisShown = false;
    let preMessage = null;

    try {
        if (input.files.length === 0) {
            preMessage = mockPreValidationMessage();
            body.replaceChildren();
            renderPreValidation(preMessage, body);
            preAnalysisShown = true;
            body.appendChild(loading);
            const analysisMessage = await requestMockAnalysis();
            body.removeChild(loading);
            renderContractResult(analysisMessage, body);
            sessionStorage.setItem(
                'analysisResult',
                JSON.stringify({ validation: preMessage, analysis: analysisMessage }),
            );
            document.getElementById('printbtn').style.visibility = 'visible';
        } else {
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
        displayUploadError(ex, body, loading, preAnalysisShown);
    } finally {
        input.value = null;
    }
}

// Registriert Event-Listener sobald das Dokument geladen ist.
document.addEventListener('DOMContentLoaded', () => {
    document
        .querySelectorAll('.upload-form')
        .forEach((form) => form.addEventListener('submit', uploadFile));
    window.printer = printer;

    const restoreToastEl = document.getElementById('restoreToast');
    const restoreToast = bootstrap.Toast.getOrCreateInstance(restoreToastEl, { autohide: false });
    const evaluationModalEl = document.getElementById('evaluation');

    evaluationModalEl.addEventListener('hidden.bs.modal', () => {
        if (sessionStorage.getItem('analysisResult')) {
            restoreToast.show();
        }
    });

    evaluationModalEl.addEventListener('show.bs.modal', () => {
        restoreToast.hide();
    });

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

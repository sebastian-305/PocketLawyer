import { startAnalysisSocket, requestMockAnalysis, mockPreValidationMessage } from './network.js';
import { renderPreValidation as rentPre, renderContractResult as rentRes } from './render-rent.js'; 
import { renderPreValidation as emploPre, renderContractResult as emploRes } from './render-employment.js';
import { createLoadingElement, displayUploadError, printer } from './render.js';

const DEBUG = true;

async function uploadFile(event) {
    event.preventDefault();

    if (DEBUG) console.debug('File upload started.');

    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    const input = event.target.myfile;
    const analysisType = event.target.contractType.value;
    let preAnalysisShown = false;
    let preMessage = null;

    try {
        if (input.files.length === 0) {
            preMessage = mockPreValidationMessage();
            body.replaceChildren();
            emploPre(preMessage, body);
            preAnalysisShown = true;
            body.appendChild(loading);
            const analysisMessage = await requestMockAnalysis();
            body.removeChild(loading);
            emploRes(analysisMessage, body);
            sessionStorage.setItem(
                'analysisResult',
                JSON.stringify({
                    analysisType,
                    validation: preMessage,
                    analysis: analysisMessage,
                }),
            );
            document.getElementById('printbtn').style.visibility = 'visible';
        } else {
            const file = input.files[0];
            const socket = startAnalysisSocket(file, event.target.contractType.value, (msg) => {
                if (msg?.error) {
                    socket.close();
                    preAnalysisShown = false;
                    const rawError = typeof msg.error === 'string' ? msg.error : '';
                    const isAiBackendError = /invalid ai response/i.test(rawError)
                        || /fehler beim interpretieren der ki-antwort/i.test(rawError)
                        || /gemini api request failed/i.test(rawError)
                        || /ki[- ]service/i.test(rawError)
                        || /ai[- ]service/i.test(rawError);
                    const friendlyMessage = 'Der KI-Service kann Ihre Anfrage derzeit nicht bearbeiten. Bitte versuchen Sie es später erneut.';
                    const errorToShow = isAiBackendError && friendlyMessage ? friendlyMessage : rawError || friendlyMessage;
                    if (DEBUG && isAiBackendError && rawError) {
                        console.info('AI backend reported error:', rawError);
                    }
                    displayUploadError(new Error(errorToShow), body, loading, preAnalysisShown);
                    return;
                }
                if (!preAnalysisShown) {
                    preMessage = msg;
                    body.replaceChildren();
                    switch (analysisType) {
                        case "rent-analysis":
                            rentPre(preMessage, body);
                            break;
                        case "employment-analysis":
                            emploPre(preMessage, body);
                            break;
                    }
                    preAnalysisShown = true;
                    if (!msg.isReadable || !msg.isContract) {
                        socket.close();
                    } else {
                        body.appendChild(loading);
                    }
                } else {
                    if(DEBUG) console.log(msg);
                    body.removeChild(loading);
                    switch (analysisType) {
                        case "rent-analysis":
                            rentRes(msg, body);
                            break;
                        case "employment-analysis":
                            emploRes(msg, body);
                            break;
                    }
                    sessionStorage.setItem(
                        'analysisResult',
                        JSON.stringify({
                            analysisType,
                            validation: preMessage,
                            analysis: msg,
                        }),
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
                const { validation, analysis, analysisType } = JSON.parse(stored);
                const body = document.getElementById('evaluation-body');
                body.replaceChildren();
                switch (analysisType) {
                    case 'rent-analysis':
                        rentPre(validation, body);
                        rentRes(analysis, body);
                        break;
                    case 'employment-analysis':
                    default:
                        emploPre(validation, body);
                        emploRes(analysis, body);
                        break;
                }
                document.getElementById('printbtn').style.visibility = 'visible';
                const modal = bootstrap.Modal.getOrCreateInstance(
                    evaluationModalEl,
                );
                modal.show();
            }
        });
});

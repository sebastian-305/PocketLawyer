import { prepareRequests, requestPreValidation, requestContractAnalysis } from './network.js';
import { createLoadingElement, renderPreValidation, renderContractResult, displayUploadError, printer } from './render.js';

async function upload_file(event) {
    event.preventDefault();

    console.debug('File upload started.');

    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    const { validationRequest, analyzeRequest, mock } = prepareRequests(event.target.myfile);

    let preAnalysisShown = false;

    try {
        const validationMessage = await requestPreValidation(validationRequest, mock);
        renderPreValidation(validationMessage, body);
        preAnalysisShown = true;

        if (!validationMessage.isReadable || !validationMessage.isContract) {
            throw new Error(`isReadable: ${validationMessage.isReadable}, isContract: ${validationMessage.isContract}`);
        }

        body.appendChild(loading);
        const analysisMessage = await requestContractAnalysis(analyzeRequest);
        body.removeChild(loading);
        renderContractResult(analysisMessage, body);
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

document.addEventListener('DOMContentLoaded', () => {
    document
        .querySelectorAll('.upload-form')
        .forEach((form) => form.addEventListener('submit', upload_file));
    window.printer = printer;

    const restoreToastEl = document.getElementById('restoreToast');
    const restoreToast = bootstrap.Toast.getOrCreateInstance(restoreToastEl);
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

import { prepareRequests, requestPreValidation, requestContractAnalysis } from './network.js';
import { createLoadingElement, renderPreValidation, renderContractResult, displayUploadError, printer } from './render.js';

async function upload_file(event) {
    event.preventDefault();

    console.info('File upload started.');

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
});

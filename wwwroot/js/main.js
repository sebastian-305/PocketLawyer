import { startAnalysisSocket, requestMockAnalysis, mockPreValidationMessage, fetchSchema } from './network.js';
import { createLoadingElement, displayUploadError, printer, createTierAccordion, renderAnalysisResult } from './render.js';

const DEBUG = true;

/**
 * Renders analysis results using the generic schema-driven renderer.
 * Creates tier accordions for Basic and Premium analyses.
 * 
 * @param {object} results - Object containing basic and premium results
 * @param {HTMLElement} body - The container element to render into
 * @param {string} contractType - The contract type ("Rental" or "Employment")
 */
async function renderGenericAnalysis(results, body, contractType) {
    if (DEBUG) console.log('Rendering generic analysis for', contractType, results);

    const container = document.createElement('div');
    container.classList.add('analysis-container');
    container.tabIndex = -1;

    // Header
    const hdr = document.createElement('div');
    hdr.classList.add('header-a');
    const heading = document.createElement('h2');
    heading.textContent = 'Deine Vertragsanalyse';
    hdr.appendChild(heading);
    container.appendChild(hdr);

    // Create tier accordion container
    const tierAccordion = document.createElement('div');
    tierAccordion.classList.add('accordion', 'tier-accordion');
    tierAccordion.id = 'tier-accordion';

    // Render Basic Analysis
    if (results.basic) {
        try {
            const basicContainer = document.createElement('div');
            const basicData = results.basic;
            
            // Build schema dynamically based on the actual data structure
            const basicSchema = await buildSchemaFromData(basicData, contractType, 'Basic');
            
            // Render with schema
            renderAnalysisResult(basicData, basicSchema, basicContainer);
            
            const basicAccordion = createTierAccordion(
                'tier-basic',
                getTierTitle('Basic'),
                basicContainer,
                true // expanded by default
            );
            
            tierAccordion.appendChild(basicAccordion);
        } catch (error) {
            if (DEBUG) console.error('Error rendering basic analysis:', error);
        }
    }

    // Render Premium Analysis
    if (results.premium) {
        try {
            const premiumContainer = document.createElement('div');
            const premiumData = results.premium;
            
            // Build schema dynamically based on the actual data structure
            const premiumSchema = await buildSchemaFromData(premiumData, contractType, 'Premium');
            
            // Render with schema
            renderAnalysisResult(premiumData, premiumSchema, premiumContainer);
            
            const premiumAccordion = createTierAccordion(
                'tier-premium',
                getTierTitle('Premium'),
                premiumContainer,
                false // collapsed by default
            );
            
            tierAccordion.appendChild(premiumAccordion);
        } catch (error) {
            if (DEBUG) console.error('Error rendering premium analysis:', error);
        }
    }

    container.appendChild(tierAccordion);
    body.appendChild(container);
    container.focus();
}

/**
 * Configuration for tier titles.
 */
const ANALYSIS_CONFIG = {
    tierTitles: {
        'Basic': 'Basis Analyse',
        'Premium': 'Premium Analyse'
    }
};

/**
 * Gets the tier title from configuration.
 * 
 * @param {string} tier - The tier ("Basic" or "Premium")
 * @returns {string} The user-friendly tier title
 */
function getTierTitle(tier) {
    return ANALYSIS_CONFIG.tierTitles[tier] || tier;
}

/**
 * Builds a dynamic schema based on the actual data structure.
 * Fetches schemas for each key in the data and combines them.
 * 
 * @param {object} data - The analysis data
 * @param {string} contractType - The contract type ("Rental" or "Employment")
 * @param {string} tier - The tier ("Basic" or "Premium")
 * @returns {Promise<object>} The combined schema
 */
async function buildSchemaFromData(data, contractType, tier) {
    if (!data || typeof data !== 'object') {
        return { type: 'object', properties: {} };
    }

    const combinedSchema = {
        type: 'object',
        properties: {}
    };

    // Fetch schemas for each key in the data
    // Data keys now match step names directly (e.g., "General", "Rent", "Conditions")
    const schemaPromises = Object.keys(data).map(async (stepName) => {
        try {
            const schema = await fetchSchema(contractType, tier, stepName);
            return { stepName, schema };
        } catch (err) {
            if (DEBUG) console.warn(`Failed to fetch schema for ${contractType}/${tier}/${stepName}:`, err);
            return { stepName, schema: null };
        }
    });

    const schemaResults = await Promise.all(schemaPromises);

    // Build combined schema
    schemaResults.forEach(({ stepName, schema }) => {
        if (schema) {
            // Use x-sectionTitle from schema if available, otherwise use the step name
            const sectionTitle = schema['x-sectionTitle'] || stepName;
            
            combinedSchema.properties[stepName] = {
                type: 'object',
                'x-displayName': sectionTitle,
                'x-styling': schema['x-styling'] || {},
                properties: schema.properties || {}
            };
        } else {
            // Fallback: create a basic schema for this step
            combinedSchema.properties[stepName] = {
                type: 'object',
                'x-displayName': stepName,
                properties: {}
            };
        }
    });

    if (DEBUG) console.log(`Built schema from data for ${contractType} ${tier}:`, combinedSchema);
    return combinedSchema;
}

/**
 * Renders data without schema (fallback method).
 * Creates simple field displays for all properties.
 * 
 * @param {object} data - The data to render
 * @param {HTMLElement} container - The container to render into
 */
function renderDataWithoutSchema(data, container) {
    if (!data || typeof data !== 'object') {
        container.textContent = String(data || 'N/A');
        return;
    }

    // Skip metadata properties from IAnalysisDto interface
    const skipProperties = ['contractType', 'tier'];

    for (const [key, value] of Object.entries(data)) {
        // Skip interface properties
        if (skipProperties.includes(key)) {
            continue;
        }

        const fieldWrapper = document.createElement('div');
        fieldWrapper.classList.add('field-item');

        const label = document.createElement('strong');
        label.classList.add('field-label');
        label.textContent = `${key}:`;
        fieldWrapper.appendChild(label);

        const valueElement = document.createElement('span');
        valueElement.classList.add('field-value');

        if (value === null || value === undefined) {
            valueElement.textContent = 'N/A';
        } else if (typeof value === 'object') {
            // Nested object or array
            const nestedContainer = document.createElement('div');
            nestedContainer.classList.add('field-nested');
            renderDataWithoutSchema(value, nestedContainer);
            fieldWrapper.appendChild(nestedContainer);
        } else {
            valueElement.textContent = String(value);
            fieldWrapper.appendChild(valueElement);
        }

        container.appendChild(fieldWrapper);
    }
}

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
            // Mock analysis for testing without file upload
            preMessage = mockPreValidationMessage();
            body.replaceChildren();
            
            const confirmationDiv = document.createElement('div');
            confirmationDiv.classList.add('alert', 'alert-success');
            confirmationDiv.innerHTML = '<strong>✓ Mock-Analyse</strong><br>Verwende Beispieldaten...';
            body.appendChild(confirmationDiv);
            body.appendChild(loading);
            
            const analysisMessage = await requestMockAnalysis('employment');
            body.removeChild(loading);
            
            // Render mock analysis using generic renderer
            await renderGenericAnalysis(analysisMessage, body, 'Employment');
            
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
            let confirmationReceived = false;
            let basicResultReceived = false;
            let basicResult = null;
            let premiumResult = null;

            const socket = startAnalysisSocket(file, event.target.contractType.value, async (msg) => {
                if (DEBUG) console.log('WebSocket message received:', msg);

                // Handle error messages
                if (msg?.error) {
                    socket.close();
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
                    displayUploadError(new Error(errorToShow), body, loading, confirmationReceived);
                    return;
                }

                // Handle confirmation message (initialization successful)
                if (msg?.type === 'confirmation') {
                    if (DEBUG) console.log('Confirmation received:', msg.message);
                    confirmationReceived = true;
                    
                    // Remove loading spinner temporarily
                    if (body.contains(loading)) {
                        body.removeChild(loading);
                    }
                    
                    // Show confirmation message to user
                    const confirmationDiv = document.createElement('div');
                    confirmationDiv.classList.add('alert', 'alert-success');
                    confirmationDiv.setAttribute('role', 'alert');
                    confirmationDiv.id = 'confirmation-message';
                    confirmationDiv.innerHTML = `
                        <strong>✓ Vertrag akzeptiert</strong><br>
                        Die Analyse wurde gestartet...
                    `;
                    body.appendChild(confirmationDiv);
                    
                    // Add loading spinner back
                    body.appendChild(loading);
                    return;
                }

                // Handle result messages (basic and premium)
                if (msg?.type === 'result') {
                    if (DEBUG) console.log('Result received:', msg.data);

                    const contractTypeMap = {
                        "rental-analysis": "Rental",
                        "employment-analysis": "Employment"
                    };
                    const contractType = contractTypeMap[analysisType] || "Rental";

                    // Determine if this is basic or premium based on what we've received
                    if (!basicResultReceived) {
                        // First result is basic - extract from data.result
                        basicResult = msg.data.result;
                        basicResultReceived = true;
                        if (DEBUG) console.log('Basic result received, rendering immediately', basicResult);

                        // Remove confirmation message if present
                        const confirmationMsg = document.getElementById('confirmation-message');
                        if (confirmationMsg) {
                            body.removeChild(confirmationMsg);
                        }

                        // Remove loading spinner temporarily
                        if (body.contains(loading)) {
                            body.removeChild(loading);
                        }

                        // Render basic result immediately
                        try {
                            await renderGenericAnalysis(
                                { basic: basicResult, premium: null },
                                body,
                                contractType
                            );
                        } catch (error) {
                            if (DEBUG) console.error('Generic renderer failed for basic:', error);
                            // Fallback: render simple message
                            const errorDiv = document.createElement('div');
                            errorDiv.classList.add('alert', 'alert-warning');
                            errorDiv.textContent = 'Basis-Analyse abgeschlossen. Premium-Analyse läuft...';
                            body.appendChild(errorDiv);
                        }

                        // Add loading spinner back for premium analysis
                        body.appendChild(loading);
                        return;
                    } else {
                        // Second result is premium - extract from data.result
                        premiumResult = msg.data.result;
                        if (DEBUG) console.log('Premium result received, rendering complete analysis');

                        // Remove loading spinner
                        if (body.contains(loading)) {
                            body.removeChild(loading);
                        }

                        // Re-render with both basic and premium results
                        try {
                            // Clear body and render complete analysis
                            body.replaceChildren();
                            await renderGenericAnalysis(
                                { basic: basicResult, premium: premiumResult },
                                body,
                                contractType
                            );
                        } catch (error) {
                            if (DEBUG) console.error('Generic renderer failed for complete analysis:', error);
                            // Fallback: show error message
                            body.replaceChildren();
                            const errorDiv = document.createElement('div');
                            errorDiv.classList.add('alert', 'alert-danger');
                            errorDiv.textContent = 'Fehler beim Rendern der Analyse. Bitte laden Sie die Seite neu.';
                            body.appendChild(errorDiv);
                        }

                        // Store results
                        sessionStorage.setItem(
                            'analysisResult',
                            JSON.stringify({
                                analysisType,
                                basic: basicResult,
                                premium: premiumResult,
                            }),
                        );

                        document.getElementById('printbtn').style.visibility = 'visible';
                        socket.close();
                    }
                    return;
                }

                // Handle legacy message format (for backward compatibility)
                if (!confirmationReceived && (msg?.isReadable !== undefined || msg?.isContract !== undefined)) {
                    if (DEBUG) console.log('Legacy pre-validation message received');
                    preMessage = msg;
                    body.replaceChildren();
                    
                    // Show validation message
                    const validationDiv = document.createElement('div');
                    validationDiv.classList.add('alert', msg.isReadable && msg.isContract ? 'alert-success' : 'alert-warning');
                    if (msg.isReadable && msg.isContract) {
                        validationDiv.innerHTML = '<strong>✓ Vertrag akzeptiert</strong><br>Die Analyse wurde gestartet...';
                    } else if (!msg.isReadable) {
                        validationDiv.innerHTML = '<strong>⚠ Vertrag nicht lesbar</strong><br>Der hochgeladene Vertrag konnte nicht gelesen werden.';
                    } else {
                        validationDiv.innerHTML = '<strong>⚠ Falscher Vertragstyp</strong><br>Der hochgeladene Vertrag scheint nicht vom erwarteten Typ zu sein.';
                    }
                    body.appendChild(validationDiv);
                    
                    confirmationReceived = true;
                    if (!msg.isReadable || !msg.isContract) {
                        socket.close();
                    } else {
                        body.appendChild(loading);
                    }
                    return;
                }

                // Handle legacy result format (for backward compatibility)
                if (confirmationReceived && !msg?.type) {
                    if (DEBUG) console.log('Legacy result message received');
                    if (body.contains(loading)) {
                        body.removeChild(loading);
                    }
                    
                    // Render using generic renderer
                    const contractTypeMap = {
                        "rental-analysis": "Rental",
                        "employment-analysis": "Employment"
                    };
                    const contractType = contractTypeMap[analysisType] || "Employment";
                    
                    try {
                        await renderGenericAnalysis(msg, body, contractType);
                    } catch (error) {
                        if (DEBUG) console.error('Generic renderer failed for legacy format:', error);
                        const errorDiv = document.createElement('div');
                        errorDiv.classList.add('alert', 'alert-danger');
                        errorDiv.textContent = 'Fehler beim Rendern der Analyse.';
                        body.appendChild(errorDiv);
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
                displayUploadError(new Error('WebSocket Fehler'), body, loading, confirmationReceived);
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
    window.getTierTitle = getTierTitle;

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
        .addEventListener('click', async () => {
            const stored = sessionStorage.getItem('analysisResult');
            if (stored) {
                const data = JSON.parse(stored);
                const body = document.getElementById('evaluation-body');
                body.replaceChildren();
                
                // Check if it's new format (has basic/premium) or old format (has validation/analysis)
                if (data.basic && data.premium) {
                    // New format
                    const contractTypeMap = {
                        "rental-analysis": "Rental",
                        "employment-analysis": "Employment"
                    };
                    const contractType = contractTypeMap[data.analysisType] || "Rental";
                    
                    try {
                        await renderGenericAnalysis(
                            { basic: data.basic, premium: data.premium },
                            body,
                            contractType
                        );
                    } catch (error) {
                        if (DEBUG) console.error('Generic renderer failed on restore:', error);
                        const errorDiv = document.createElement('div');
                        errorDiv.classList.add('alert', 'alert-danger');
                        errorDiv.textContent = 'Fehler beim Wiederherstellen der Analyse.';
                        body.appendChild(errorDiv);
                    }
                } else {
                    // Old format - render using generic renderer
                    const { validation, analysis, analysisType } = data;
                    const contractTypeMap = {
                        "rental-analysis": "Rental",
                        "employment-analysis": "Employment"
                    };
                    const contractType = contractTypeMap[analysisType] || "Employment";
                    
                    if (validation) {
                        const validationDiv = document.createElement('div');
                        validationDiv.classList.add('alert', 'alert-info');
                        validationDiv.innerHTML = '<strong>Validierung:</strong> Vertrag wurde akzeptiert';
                        body.appendChild(validationDiv);
                    }
                    
                    try {
                        await renderGenericAnalysis(analysis, body, contractType);
                    } catch (error) {
                        if (DEBUG) console.error('Generic renderer failed for old format:', error);
                        const errorDiv = document.createElement('div');
                        errorDiv.classList.add('alert', 'alert-danger');
                        errorDiv.textContent = 'Fehler beim Wiederherstellen der Analyse.';
                        body.appendChild(errorDiv);
                    }
                }
                
                document.getElementById('printbtn').style.visibility = 'visible';
                const modal = bootstrap.Modal.getOrCreateInstance(
                    evaluationModalEl,
                );
                modal.show();
            }
        });
});

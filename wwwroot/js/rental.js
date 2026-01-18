/**
 * Rental Contract Analysis Module
 * 
 * This module provides rental-specific functionality for the Klausel Klara application.
 * It handles WebSocket connections, form submissions, and result rendering specifically
 * for rental contract analysis.
 * 
 * Requirements: 1.1 - Separate front-end pages with dedicated functionality
 */

import { startAnalysisSocket, requestMockAnalysis, mockPreValidationMessage, fetchSchema } from './network.js';
import { createLoadingElement, displayUploadError, printer, createTierAccordion, renderAnalysisResult } from './render.js';

const DEBUG = true;
const CONTRACT_TYPE = 'rental-analysis';
const CONTRACT_TYPE_DISPLAY = 'Rental';

/**
 * Renders rental analysis results using the generic schema-driven renderer.
 * Creates tier accordions for Basic and Premium analyses.
 * 
 * @param {object} results - Object containing basic and premium results
 * @param {HTMLElement} body - The container element to render into
 */
async function renderRentalAnalysis(results, body) {
    if (DEBUG) console.log('Rendering rental analysis', results);

    const container = document.createElement('div');
    container.classList.add('analysis-container');
    container.tabIndex = -1;

    // Header
    const hdr = document.createElement('div');
    hdr.classList.add('header-a');
    const heading = document.createElement('h2');
    heading.textContent = 'Deine Mietvertragsanalyse';
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
            const basicSchema = await buildSchemaFromData(basicData, 'Basic');
            
            // Render with schema
            renderAnalysisResult(basicData, basicSchema, basicContainer);
            
            const basicAccordion = createTierAccordion(
                'tier-basic',
                'Basis Analyse',
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
            const premiumSchema = await buildSchemaFromData(premiumData, 'Premium');
            
            // Render with schema
            renderAnalysisResult(premiumData, premiumSchema, premiumContainer);
            
            const premiumAccordion = createTierAccordion(
                'tier-premium',
                'Premium Analyse',
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
 * Builds a dynamic schema based on the actual data structure.
 * Fetches schemas for each key in the data and combines them.
 * 
 * @param {object} data - The analysis data
 * @param {string} tier - The tier ("Basic" or "Premium")
 * @returns {Promise<object>} The combined schema
 */
async function buildSchemaFromData(data, tier) {
    if (!data || typeof data !== 'object') {
        return { type: 'object', properties: {} };
    }

    const combinedSchema = {
        type: 'object',
        properties: {}
    };

    // Fetch schemas for each key in the data
    const schemaPromises = Object.keys(data).map(async (stepName) => {
        try {
            const schema = await fetchSchema(CONTRACT_TYPE_DISPLAY, tier, stepName);
            return { stepName, schema };
        } catch (err) {
            if (DEBUG) console.warn(`Failed to fetch schema for ${CONTRACT_TYPE_DISPLAY}/${tier}/${stepName}:`, err);
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

    if (DEBUG) console.log(`Built schema from data for ${CONTRACT_TYPE_DISPLAY} ${tier}:`, combinedSchema);
    return combinedSchema;
}

// Global variable to track active WebSocket connection
let activeSocket = null;

/**
 * Closes the active WebSocket connection if one exists.
 */
function closeActiveSocket() {
    if (activeSocket && activeSocket.readyState === WebSocket.OPEN) {
        if (DEBUG) console.log('Closing active WebSocket connection');
        activeSocket.close(1000, 'User closed modal');
        activeSocket = null;
    }
}

/**
 * Handles rental contract file upload and analysis.
 * Manages WebSocket communication, progress updates, and result rendering.
 * 
 * @param {Event} event - The form submit event
 */
async function uploadRentalContract(event) {
    event.preventDefault();

    if (DEBUG) console.debug('Rental contract upload started.');

    // Close any existing WebSocket connection
    closeActiveSocket();

    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    const input = event.target.myfile;
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
            
            const analysisMessage = await requestMockAnalysis();
            body.removeChild(loading);
            
            // Render mock analysis
            await renderRentalAnalysis(analysisMessage, body);
            
            sessionStorage.setItem(
                'analysisResult',
                JSON.stringify({
                    analysisType: CONTRACT_TYPE,
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

            const socket = startAnalysisSocket(file, CONTRACT_TYPE, async (msg) => {
                if (DEBUG) console.log('WebSocket message received:', msg);

                // Handle error messages
                if (msg?.error) {
                    socket.close();
                    activeSocket = null;
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
                    
                    // Remove loading spinner
                    if (body.contains(loading)) {
                        body.removeChild(loading);
                    }
                    
                    // Show error message
                    const errorDiv = document.createElement('div');
                    errorDiv.classList.add('alert', 'alert-danger');
                    errorDiv.setAttribute('role', 'alert');
                    errorDiv.innerHTML = `<strong>⚠ Fehler</strong><br>${errorToShow}`;
                    body.appendChild(errorDiv);
                    
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
                        <strong>✓ Mietvertrag akzeptiert</strong><br>
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
                            await renderRentalAnalysis(
                                { basic: basicResult, premium: null },
                                body
                            );
                        } catch (error) {
                            if (DEBUG) console.error('Renderer failed for basic:', error);
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
                            await renderRentalAnalysis(
                                { basic: basicResult, premium: premiumResult },
                                body
                            );
                        } catch (error) {
                            if (DEBUG) console.error('Renderer failed for complete analysis:', error);
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
                                analysisType: CONTRACT_TYPE,
                                basic: basicResult,
                                premium: premiumResult,
                            }),
                        );

                        document.getElementById('printbtn').style.visibility = 'visible';
                        socket.close();
                        activeSocket = null;
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
                        validationDiv.innerHTML = '<strong>✓ Mietvertrag akzeptiert</strong><br>Die Analyse wurde gestartet...';
                    } else if (!msg.isReadable) {
                        validationDiv.innerHTML = '<strong>⚠ Vertrag nicht lesbar</strong><br>Der hochgeladene Vertrag konnte nicht gelesen werden.';
                    } else {
                        validationDiv.innerHTML = '<strong>⚠ Falscher Vertragstyp</strong><br>Der hochgeladene Vertrag scheint kein Mietvertrag zu sein.';
                    }
                    body.appendChild(validationDiv);
                    
                    confirmationReceived = true;
                    if (!msg.isReadable || !msg.isContract) {
                        socket.close();
                        activeSocket = null;
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
                    
                    // Render using rental renderer
                    try {
                        await renderRentalAnalysis(msg, body);
                    } catch (error) {
                        if (DEBUG) console.error('Renderer failed for legacy format:', error);
                        const errorDiv = document.createElement('div');
                        errorDiv.classList.add('alert', 'alert-danger');
                        errorDiv.textContent = 'Fehler beim Rendern der Analyse.';
                        body.appendChild(errorDiv);
                    }
                    
                    sessionStorage.setItem(
                        'analysisResult',
                        JSON.stringify({
                            analysisType: CONTRACT_TYPE,
                            validation: preMessage,
                            analysis: msg,
                        }),
                    );
                    document.getElementById('printbtn').style.visibility = 'visible';
                    socket.close();
                    activeSocket = null;
                }
            });

            // Store the active socket
            activeSocket = socket;

            socket.addEventListener('error', () => {
                displayUploadError(new Error('WebSocket Fehler'), body, loading, confirmationReceived);
                activeSocket = null;
            });

            socket.addEventListener('close', (event) => {
                if (DEBUG) console.log('WebSocket connection closed', event.code, event.reason);
                activeSocket = null;
                
                // If socket closed and we're still showing loading spinner
                if (body.contains(loading)) {
                    // Remove loading spinner
                    body.removeChild(loading);
                    
                    // Show error message if not normal closure
                    if (event.code !== 1000) {
                        const errorDiv = document.createElement('div');
                        errorDiv.classList.add('alert', 'alert-danger');
                        errorDiv.setAttribute('role', 'alert');
                        
                        if (event.reason) {
                            errorDiv.innerHTML = `<strong>⚠ Fehler</strong><br>${event.reason}`;
                        } else if (event.code === 1003) {
                            errorDiv.innerHTML = '<strong>⚠ Ungültige Daten</strong><br>Der hochgeladene Vertrag konnte nicht verarbeitet werden.';
                        } else if (event.code === 1011) {
                            errorDiv.innerHTML = '<strong>⚠ Serverfehler</strong><br>Bei der Verarbeitung ist ein Fehler aufgetreten.';
                        } else {
                            errorDiv.innerHTML = '<strong>⚠ Verbindung unterbrochen</strong><br>Die Verbindung zum Server wurde unerwartet geschlossen.';
                        }
                        
                        body.appendChild(errorDiv);
                    }
                }
            });
        }
    } catch (ex) {
        displayUploadError(ex, body, loading, preAnalysisShown);
    } finally {
        input.value = null;
    }
}

/**
 * Restores a previously saved rental analysis from session storage.
 * Handles both new format (basic/premium) and old format (validation/analysis).
 */
async function restoreRentalAnalysis() {
    const stored = sessionStorage.getItem('analysisResult');
    if (!stored) {
        return;
    }

    const data = JSON.parse(stored);
    
    // Only restore if it's a rental analysis
    if (data.analysisType !== CONTRACT_TYPE) {
        return;
    }

    const body = document.getElementById('evaluation-body');
    body.replaceChildren();
    
    // Check if it's new format (has basic/premium) or old format (has validation/analysis)
    if (data.basic && data.premium) {
        // New format
        try {
            await renderRentalAnalysis(
                { basic: data.basic, premium: data.premium },
                body
            );
        } catch (error) {
            if (DEBUG) console.error('Renderer failed on restore:', error);
            const errorDiv = document.createElement('div');
            errorDiv.classList.add('alert', 'alert-danger');
            errorDiv.textContent = 'Fehler beim Wiederherstellen der Analyse.';
            body.appendChild(errorDiv);
        }
    } else {
        // Old format - render using rental renderer
        const { validation, analysis } = data;
        
        if (validation) {
            const validationDiv = document.createElement('div');
            validationDiv.classList.add('alert', 'alert-info');
            validationDiv.innerHTML = '<strong>Validierung:</strong> Mietvertrag wurde akzeptiert';
            body.appendChild(validationDiv);
        }
        
        try {
            await renderRentalAnalysis(analysis, body);
        } catch (error) {
            if (DEBUG) console.error('Renderer failed for old format:', error);
            const errorDiv = document.createElement('div');
            errorDiv.classList.add('alert', 'alert-danger');
            errorDiv.textContent = 'Fehler beim Wiederherstellen der Analyse.';
            body.appendChild(errorDiv);
        }
    }
    
    document.getElementById('printbtn').style.visibility = 'visible';
}

/**
 * Initializes the rental contract analysis page.
 * Sets up event listeners for form submission and analysis restoration.
 */
function initializeRentalPage() {
    if (DEBUG) console.log('Initializing rental contract analysis page');

    // Set up form submission handlers
    document
        .querySelectorAll('.upload-form')
        .forEach((form) => form.addEventListener('submit', uploadRentalContract));

    // Set up printer function
    window.printer = printer;

    // Set up toast for restoring analysis
    const restoreToastEl = document.getElementById('restoreToast');
    const restoreToast = bootstrap.Toast.getOrCreateInstance(restoreToastEl, { autohide: false });
    const evaluationModalEl = document.getElementById('evaluation');

    evaluationModalEl.addEventListener('hidden.bs.modal', () => {
        // Close active WebSocket when modal is closed
        closeActiveSocket();
        
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
            await restoreRentalAnalysis();
            const modal = bootstrap.Modal.getOrCreateInstance(evaluationModalEl);
            modal.show();
        });

    if (DEBUG) console.log('Rental page initialized successfully');
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', initializeRentalPage);

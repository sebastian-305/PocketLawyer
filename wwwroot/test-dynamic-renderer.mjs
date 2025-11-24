import { renderAnalysisResult, createTierAccordion } from './render.js';

// Test data and schemas
const rentalBasicData = {
    vertragstyp: "Mietvertrag Typ I",
    mieter: "Max Mustermann",
    vermieter: "Immobilien GmbH",
    mietobjektAdresse: "Musterstraße 123, 1010 Wien",
    vertragsbeginn: "2024-01-01",
    vertragsart: "Unbefristet",
    kaltmiete: 850.50,
    nebenkosten: 150.00,
    warmmiete: 1000.50,
    kaution: 2550.00,
    zahlungsweise: "Monatlich im Voraus"
};

const rentalBasicSchema = {
    type: "object",
    properties: {
        vertragstyp: {
            type: "string",
            "x-title": "Vertragstyp"
        },
        mieter: {
            type: "string",
            "x-title": "Mieter"
        },
        vermieter: {
            type: "string",
            "x-title": "Vermieter"
        },
        mietobjektAdresse: {
            type: "string",
            "x-title": "Adresse des Mietobjekts"
        },
        vertragsbeginn: {
            type: "string",
            "x-title": "Vertragsbeginn"
        },
        vertragsart: {
            type: "string",
            "x-title": "Vertragsart"
        },
        kaltmiete: {
            type: "number",
            "x-title": "Kaltmiete (€)"
        },
        nebenkosten: {
            type: "number",
            "x-title": "Nebenkosten (€)"
        },
        warmmiete: {
            type: "number",
            "x-title": "Warmmiete (€)"
        },
        kaution: {
            type: "number",
            "x-title": "Kaution (€)"
        },
        zahlungsweise: {
            type: "string",
            "x-title": "Zahlungsweise"
        }
    }
};

const rentalPremiumData = {
    mietzahlungUndNebenkosten: {
        zusammenfassung: "Die Mietzahlungs- und Nebenkostenregelungen sind grundsätzlich zulässig.",
        bewertung: "Die Höhe der Nebenkosten liegt im üblichen Rahmen.",
        rechtsgrundlage: "§ 21 MRG",
        risikoScore: "Zulaessig",
        riskLevel: "Gruen"
    },
    mietdauerUndKuendigung: {
        zusammenfassung: "Die Kündigungsfristen entsprechen den gesetzlichen Vorgaben.",
        bewertung: "Keine Auffälligkeiten festgestellt.",
        rechtsgrundlage: "§ 29 MRG",
        risikoScore: "Zulaessig",
        riskLevel: "Gruen"
    },
    gesamtfazit: "Der Vertrag ist insgesamt als rechtlich unbedenklich einzustufen.",
    gesamtbewertung: "Gruen"
};

const rentalPremiumSchema = {
    type: "object",
    properties: {
        mietzahlungUndNebenkosten: {
            type: "object",
            "x-title": "Mietzahlung und Nebenkosten",
            properties: {
                zusammenfassung: { type: "string", "x-title": "Zusammenfassung" },
                bewertung: { type: "string", "x-title": "Bewertung" },
                rechtsgrundlage: { type: "string", "x-title": "Rechtsgrundlage" },
                risikoScore: { type: "string", "x-title": "Risiko-Score" },
                riskLevel: { type: "string", "x-title": "Risiko-Level" }
            }
        },
        mietdauerUndKuendigung: {
            type: "object",
            "x-title": "Mietdauer und Kündigung",
            properties: {
                zusammenfassung: { type: "string", "x-title": "Zusammenfassung" },
                bewertung: { type: "string", "x-title": "Bewertung" },
                rechtsgrundlage: { type: "string", "x-title": "Rechtsgrundlage" },
                risikoScore: { type: "string", "x-title": "Risiko-Score" },
                riskLevel: { type: "string", "x-title": "Risiko-Level" }
            }
        },
        gesamtfazit: {
            type: "string",
            "x-title": "Gesamtfazit"
        },
        gesamtbewertung: {
            type: "string",
            "x-title": "Gesamtbewertung"
        }
    }
};

// Test functions
window.runTest1 = function() {
    const container = document.getElementById('test1-container');
    container.innerHTML = '';
    console.log('Running Test 1: Basic Rental Analysis');
    renderAnalysisResult(rentalBasicData, rentalBasicSchema, container);
};

window.runTest2 = function() {
    const container = document.getElementById('test2-container');
    container.innerHTML = '';
    console.log('Running Test 2: Premium Rental Analysis');
    renderAnalysisResult(rentalPremiumData, rentalPremiumSchema, container);
};

window.runTest3 = function() {
    const container = document.getElementById('test3-container');
    container.innerHTML = '';
    console.log('Running Test 3: Missing/Null Fields');
    
    const dataWithNulls = {
        vertragstyp: "Mietvertrag Typ I",
        mieter: null,
        vermieter: undefined,
        mietobjektAdresse: "Musterstraße 123",
        kaltmiete: 0,
        nebenkosten: null
    };
    
    renderAnalysisResult(dataWithNulls, rentalBasicSchema, container);
};

window.runTest4 = function() {
    const container = document.getElementById('test4-container');
    container.innerHTML = '';
    console.log('Running Test 4: Deeply Nested Objects');
    
    const deeplyNestedData = {
        level1: {
            level2: {
                level3: {
                    value: "Deep value",
                    number: 42
                },
                anotherField: "Level 2 value"
            },
            simpleField: "Level 1 value"
        },
        topLevel: "Top level value"
    };
    
    const deepSchema = {
        type: "object",
        properties: {
            level1: {
                type: "object",
                "x-title": "Ebene 1",
                properties: {
                    level2: {
                        type: "object",
                        "x-title": "Ebene 2",
                        properties: {
                            level3: {
                                type: "object",
                                "x-title": "Ebene 3",
                                properties: {
                                    value: { type: "string", "x-title": "Wert" },
                                    number: { type: "number", "x-title": "Nummer" }
                                }
                            },
                            anotherField: { type: "string", "x-title": "Weiteres Feld" }
                        }
                    },
                    simpleField: { type: "string", "x-title": "Einfaches Feld" }
                }
            },
            topLevel: { type: "string", "x-title": "Oberste Ebene" }
        }
    };
    
    renderAnalysisResult(deeplyNestedData, deepSchema, container);
};

window.runTest5 = function() {
    const container = document.getElementById('test5-container');
    container.innerHTML = '';
    console.log('Running Test 5: Arrays and Lists');
    
    const arrayData = {
        einfacheListe: ["Item 1", "Item 2", "Item 3"],
        objektListe: [
            { name: "Objekt 1", wert: 100 },
            { name: "Objekt 2", wert: 200 }
        ],
        gemischteListe: [1, "text", true, null]
    };
    
    const arraySchema = {
        type: "object",
        properties: {
            einfacheListe: {
                type: "array",
                "x-title": "Einfache Liste",
                items: { type: "string" }
            },
            objektListe: {
                type: "array",
                "x-title": "Objekt-Liste",
                items: {
                    type: "object",
                    properties: {
                        name: { type: "string", "x-title": "Name" },
                        wert: { type: "number", "x-title": "Wert" }
                    }
                }
            },
            gemischteListe: {
                type: "array",
                "x-title": "Gemischte Liste"
            }
        }
    };
    
    renderAnalysisResult(arrayData, arraySchema, container);
};

window.runTest6 = function() {
    const container = document.getElementById('test6-container');
    container.innerHTML = '';
    console.log('Running Test 6: Missing x-title (Fallback)');
    
    const data = {
        fieldWithoutTitle: "This field has no x-title",
        anotherField: "Another field without x-title",
        numberField: 12345
    };
    
    const schemaWithoutTitles = {
        type: "object",
        properties: {
            fieldWithoutTitle: { type: "string" },
            anotherField: { type: "string" },
            numberField: { type: "number" }
        }
    };
    
    renderAnalysisResult(data, schemaWithoutTitles, container);
};

window.runTest7 = function() {
    const container = document.getElementById('test7-container');
    container.innerHTML = '';
    console.log('Running Test 7: Full Analysis with Tier Accordions');
    
    // Create tier accordion container
    const tierAccordion = document.createElement('div');
    tierAccordion.classList.add('accordion', 'tier-accordion');
    tierAccordion.id = 'test-tier-accordion';
    
    // Basic tier
    const basicContainer = document.createElement('div');
    renderAnalysisResult(rentalBasicData, rentalBasicSchema, basicContainer);
    const basicAccordion = createTierAccordion('test-tier-basic', 'Basis Analyse', basicContainer, true);
    tierAccordion.appendChild(basicAccordion);
    
    // Premium tier
    const premiumContainer = document.createElement('div');
    renderAnalysisResult(rentalPremiumData, rentalPremiumSchema, premiumContainer);
    const premiumAccordion = createTierAccordion('test-tier-premium', 'Premium Analyse', premiumContainer, false);
    tierAccordion.appendChild(premiumAccordion);
    
    container.appendChild(tierAccordion);
};

window.runTest8 = function() {
    const container = document.getElementById('test8-container');
    const logContainer = document.getElementById('test8-log');
    container.innerHTML = '';
    logContainer.innerHTML = '';
    
    console.log('Running Test 8: WebSocket Message Flow Simulation');
    
    function log(message) {
        const timestamp = new Date().toLocaleTimeString();
        logContainer.innerHTML += `[${timestamp}] ${message}<br>`;
    }
    
    log('Simulating WebSocket connection...');
    
    // Simulate confirmation message
    setTimeout(() => {
        log('✓ Received: confirmation message');
        const confirmDiv = document.createElement('div');
        confirmDiv.style.padding = '1rem';
        confirmDiv.style.backgroundColor = '#d4edda';
        confirmDiv.style.borderRadius = '5px';
        confirmDiv.style.marginBottom = '1rem';
        confirmDiv.textContent = '✓ Vertrag akzeptiert, Analyse gestartet';
        container.appendChild(confirmDiv);
    }, 500);
    
    // Simulate basic result
    setTimeout(() => {
        log('✓ Received: basic result');
        const basicDiv = document.createElement('div');
        basicDiv.style.marginBottom = '1rem';
        const basicTitle = document.createElement('h4');
        basicTitle.textContent = 'Basic Analysis Result';
        basicTitle.style.color = 'var(--blue-bg)';
        basicDiv.appendChild(basicTitle);
        renderAnalysisResult(rentalBasicData, rentalBasicSchema, basicDiv);
        container.appendChild(basicDiv);
    }, 1500);
    
    // Simulate premium result
    setTimeout(() => {
        log('✓ Received: premium result');
        const premiumDiv = document.createElement('div');
        const premiumTitle = document.createElement('h4');
        premiumTitle.textContent = 'Premium Analysis Result';
        premiumTitle.style.color = 'var(--blue-bg)';
        premiumDiv.appendChild(premiumTitle);
        renderAnalysisResult(rentalPremiumData, rentalPremiumSchema, premiumDiv);
        container.appendChild(premiumDiv);
        log('✓ Analysis complete, connection closed');
    }, 2500);
};

window.clearTest = function(containerId) {
    const container = document.getElementById(containerId);
    container.innerHTML = '';
    if (containerId === 'test8-container') {
        document.getElementById('test8-log').innerHTML = '';
    }
};

console.log('Dynamic Renderer Test Suite loaded');

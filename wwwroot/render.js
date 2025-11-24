const DEBUG = true;

export function printer() {
    const printWindow = window.open(
        "about:blank",
        "Vertragsanalyse",
        "left=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0",
    );

    const doc = printWindow.document;
    doc.title = "Vertragsanalyse";

    const link = doc.createElement("link");
    link.rel = "stylesheet";
    link.href = "print.css";
    link.addEventListener("load", () => {
        printWindow.focus();
        printWindow.print();
        printWindow.close();
    });
    doc.head.appendChild(link);

    const content = document.getElementById("evaluation-body").cloneNode(true);
    doc.body.appendChild(content);
}

export function createLoadingElement() {
    const loading = document.createElement("div");
    loading.classList.add("loading-spinner");
    loading.setAttribute("role", "status");
    const srText = document.createElement("span");
    srText.classList.add("visually-hidden");
    srText.textContent = "Analyse läuft…";
    loading.appendChild(srText);
    return loading;
}

export function displayUploadError(ex, body, loading, preAnalysisShown) {
    if (DEBUG) console.error(ex);
    if (body.contains(loading)) {
        body.removeChild(loading);
    }
    if (!preAnalysisShown) {
        body.replaceChildren();
    }
    const err = document.createElement("div");
    err.classList.add("error-message");

    err.setAttribute("role", "alert");
    err.setAttribute("tabindex", "-1");

    err.textContent = `Fehler: ${ex.message}`;
    body.appendChild(err);
    err.focus();
}

/**
 * Creates a Bootstrap accordion item for analysis tiers (Basic/Premium).
 * 
 * @param {string} id - Unique ID for the accordion item
 * @param {string} title - The title to display in the accordion header
 * @param {HTMLElement} content - The content to display in the accordion body
 * @param {boolean} expanded - Whether the accordion should be expanded by default
 * @returns {HTMLElement} The accordion item element
 */
export function createTierAccordion(id, title, content, expanded = true) {
    const item = document.createElement('div');
    item.classList.add('accordion-item');

    const header = document.createElement('h2');
    header.classList.add('accordion-header');

    const button = document.createElement('button');
    button.classList.add('accordion-button');
    if (!expanded) {
        button.classList.add('collapsed');
    }
    button.type = 'button';
    button.setAttribute('data-bs-toggle', 'collapse');
    button.setAttribute('data-bs-target', `#${id}`);
    button.setAttribute('aria-expanded', expanded ? 'true' : 'false');
    button.setAttribute('aria-controls', id);
    button.textContent = title;

    header.appendChild(button);
    item.appendChild(header);

    const collapse = document.createElement('div');
    collapse.id = id;
    collapse.classList.add('accordion-collapse', 'collapse');
    if (expanded) {
        collapse.classList.add('show');
    }

    const body = document.createElement('div');
    body.classList.add('accordion-body');
    body.appendChild(content);

    collapse.appendChild(body);
    item.appendChild(collapse);

    return item;
}

/**
 * Renders analysis results dynamically based on JSON data and schema metadata.
 * Creates accordion sections for each root key in the result object.
 * 
 * @param {object} data - The analysis result data to render
 * @param {object} schema - The JSON schema with x-title metadata for field labels
 * @param {HTMLElement} container - The DOM element to render into
 */
export function renderAnalysisResult(data, schema, container) {
    if (!data || !schema || !container) {
        if (DEBUG) console.error('renderAnalysisResult: Missing required parameters', { data, schema, container });
        return;
    }

    // Clear the container
    container.innerHTML = '';

    // Get schema properties
    const schemaProperties = schema.properties || {};

    // Create an accordion container for sections
    const sectionAccordion = document.createElement('div');
    sectionAccordion.classList.add('accordion', 'section-accordion');
    sectionAccordion.id = `accordion-${Date.now()}`;

    let sectionIndex = 0;

    // Render each property in the data as an accordion section
    for (const [key, value] of Object.entries(data)) {
        const propertySchema = schemaProperties[key];
        
        // Get the display label from x-title or fallback to key name
        const label = propertySchema?.['x-title'] || key;

        // Create accordion section for this root key
        const sectionId = `section-${Date.now()}-${sectionIndex}`;
        const accordionItem = createSectionAccordion(sectionId, label, value, propertySchema, sectionIndex === 0);
        
        if (accordionItem) {
            sectionAccordion.appendChild(accordionItem);
        }
        
        sectionIndex++;
    }

    container.appendChild(sectionAccordion);
}

/**
 * Creates an accordion item for a data section.
 * 
 * @param {string} id - Unique ID for the accordion item
 * @param {string} title - The section title
 * @param {*} value - The section data
 * @param {object} propertySchema - The schema for this section
 * @param {boolean} expanded - Whether the section should be expanded by default
 * @returns {HTMLElement} The accordion item element
 */
function createSectionAccordion(id, title, value, propertySchema, expanded = false) {
    const item = document.createElement('div');
    item.classList.add('accordion-item');

    const header = document.createElement('h2');
    header.classList.add('accordion-header');

    const button = document.createElement('button');
    button.classList.add('accordion-button');
    if (!expanded) {
        button.classList.add('collapsed');
    }
    button.type = 'button';
    button.setAttribute('data-bs-toggle', 'collapse');
    button.setAttribute('data-bs-target', `#${id}`);
    button.setAttribute('aria-expanded', expanded ? 'true' : 'false');
    button.setAttribute('aria-controls', id);
    button.textContent = title;

    header.appendChild(button);
    item.appendChild(header);

    const collapse = document.createElement('div');
    collapse.id = id;
    collapse.classList.add('accordion-collapse', 'collapse');
    if (expanded) {
        collapse.classList.add('show');
    }

    const body = document.createElement('div');
    body.classList.add('accordion-body', 'section');

    // Render the content based on type
    const content = renderSectionContent(value, propertySchema);
    body.appendChild(content);

    collapse.appendChild(body);
    item.appendChild(collapse);

    return item;
}

/**
 * Renders the content of a section.
 * 
 * @param {*} value - The section data
 * @param {object} propertySchema - The schema for this section
 * @returns {HTMLElement} The rendered content
 */
function renderSectionContent(value, propertySchema) {
    const container = document.createElement('div');
    
    // Handle null/undefined
    if (value === null || value === undefined) {
        container.textContent = 'N/A';
        return container;
    }

    // Handle arrays
    if (Array.isArray(value)) {
        const listElement = renderArrayContent(value, propertySchema);
        container.appendChild(listElement);
        return container;
    }

    // Handle objects
    if (typeof value === 'object') {
        const objectContent = renderObjectContent(value, propertySchema);
        container.appendChild(objectContent);
        return container;
    }

    // Handle primitives
    container.textContent = String(value);
    return container;
}

/**
 * Renders object content with nested fields.
 * 
 * @param {object} value - The object value
 * @param {object} propertySchema - The schema for this object
 * @returns {HTMLElement} The rendered object content
 */
function renderObjectContent(value, propertySchema) {
    const container = document.createElement('div');
    const nestedProperties = propertySchema?.properties || {};

    for (const [nestedKey, nestedValue] of Object.entries(value)) {
        const nestedPropertySchema = nestedProperties[nestedKey];
        const nestedLabel = nestedPropertySchema?.['x-title'] || nestedKey;
        const fieldElement = renderField(nestedValue, nestedLabel, nestedPropertySchema);
        
        if (fieldElement) {
            container.appendChild(fieldElement);
        }
    }

    return container;
}

/**
 * Renders array content.
 * 
 * @param {Array} value - The array value
 * @param {object} propertySchema - The schema for this array
 * @returns {HTMLElement} The rendered array content
 */
function renderArrayContent(value, propertySchema) {
    const listElement = document.createElement('ul');
    listElement.classList.add('field-array-list');

    value.forEach((item) => {
        const listItem = document.createElement('li');
        
        if (typeof item === 'object' && item !== null) {
            const itemSchema = propertySchema?.items;
            const itemContainer = document.createElement('div');
            
            if (itemSchema?.properties) {
                for (const [itemKey, itemValue] of Object.entries(item)) {
                    const itemPropertySchema = itemSchema.properties[itemKey];
                    const itemLabel = itemPropertySchema?.['x-title'] || itemKey;
                    const fieldElement = renderField(itemValue, itemLabel, itemPropertySchema);
                    if (fieldElement) {
                        itemContainer.appendChild(fieldElement);
                    }
                }
            } else {
                itemContainer.textContent = JSON.stringify(item);
            }
            
            listItem.appendChild(itemContainer);
        } else {
            listItem.textContent = String(item);
        }
        
        listElement.appendChild(listItem);
    });

    return listElement;
}

/**
 * Renders a single field based on its type and schema.
 * 
 * @param {*} value - The field value
 * @param {string} label - The display label for the field
 * @param {object} propertySchema - The schema definition for this property
 * @returns {HTMLElement|null} The rendered field element
 */
function renderField(value, label, propertySchema) {
    // Handle null/undefined values
    if (value === null || value === undefined) {
        return renderPrimitiveField(label, 'N/A');
    }

    // Determine the type
    const schemaType = propertySchema?.type;

    // Handle arrays
    if (Array.isArray(value)) {
        return renderArrayField(label, value, propertySchema);
    }

    // Handle objects (nested structures)
    if (typeof value === 'object' && schemaType === 'object') {
        return renderObjectField(label, value, propertySchema);
    }

    // Handle primitive values (string, number, boolean)
    return renderPrimitiveField(label, value);
}

/**
 * Renders a primitive field (string, number, boolean).
 * 
 * @param {string} label - The field label
 * @param {*} value - The field value
 * @returns {HTMLElement} The rendered field element
 */
function renderPrimitiveField(label, value) {
    const wrapper = document.createElement('div');
    wrapper.classList.add('field-item');

    const labelElement = document.createElement('strong');
    labelElement.classList.add('field-label');
    labelElement.textContent = `${label}:`;
    wrapper.appendChild(labelElement);

    const valueElement = document.createElement('span');
    valueElement.classList.add('field-value');
    
    // Format the value based on type
    const formattedValue = formatValue(value);
    valueElement.textContent = formattedValue;
    
    wrapper.appendChild(valueElement);

    return wrapper;
}

/**
 * Formats a value for display based on its type.
 * 
 * @param {*} value - The value to format
 * @returns {string} The formatted value
 */
function formatValue(value) {
    // Handle null/undefined
    if (value === null || value === undefined) {
        return 'N/A';
    }
    
    // Handle booleans
    if (typeof value === 'boolean') {
        return value ? 'Ja' : 'Nein';
    }
    
    // Handle numbers
    if (typeof value === 'number') {
        // Check if it's a currency value (has decimal places or is large)
        if (Number.isInteger(value) && value < 1000) {
            return value.toString();
        }
        // Format as currency or decimal
        return new Intl.NumberFormat('de-DE', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        }).format(value);
    }
    
    // Handle dates (ISO string format)
    if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value)) {
        try {
            const date = new Date(value);
            if (!isNaN(date.getTime())) {
                return new Intl.DateTimeFormat('de-DE', {
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit'
                }).format(date);
            }
        } catch (e) {
            // Not a valid date, continue to string handling
        }
    }
    
    // Handle strings
    return String(value);
}

/**
 * Renders an array field.
 * 
 * @param {string} label - The field label
 * @param {Array} value - The array value
 * @param {object} propertySchema - The schema definition for this property
 * @returns {HTMLElement} The rendered field element
 */
function renderArrayField(label, value, propertySchema) {
    const wrapper = document.createElement('div');
    wrapper.classList.add('field-item', 'field-array');

    const labelElement = document.createElement('strong');
    labelElement.textContent = `${label}:`;
    wrapper.appendChild(labelElement);

    const listElement = document.createElement('ul');
    listElement.classList.add('field-array-list');

    value.forEach((item) => {
        const listItem = document.createElement('li');
        
        // If array items are objects, render them recursively
        if (typeof item === 'object' && item !== null) {
            const itemSchema = propertySchema?.items;
            const itemContainer = document.createElement('div');
            
            if (itemSchema?.properties) {
                // Render nested object
                for (const [itemKey, itemValue] of Object.entries(item)) {
                    const itemPropertySchema = itemSchema.properties[itemKey];
                    const itemLabel = itemPropertySchema?.['x-title'] || itemKey;
                    const fieldElement = renderField(itemValue, itemLabel, itemPropertySchema);
                    if (fieldElement) {
                        itemContainer.appendChild(fieldElement);
                    }
                }
            } else {
                // No schema, just render as JSON
                itemContainer.textContent = JSON.stringify(item);
            }
            
            listItem.appendChild(itemContainer);
        } else {
            // Primitive array item
            listItem.textContent = String(item);
        }
        
        listElement.appendChild(listItem);
    });

    wrapper.appendChild(listElement);
    return wrapper;
}

/**
 * Renders an object field (nested structure).
 * For complex nested objects, creates a nested accordion.
 * 
 * @param {string} label - The field label
 * @param {object} value - The object value
 * @param {object} propertySchema - The schema definition for this property
 * @returns {HTMLElement} The rendered field element
 */
function renderObjectField(label, value, propertySchema) {
    const wrapper = document.createElement('div');
    wrapper.classList.add('field-item', 'field-object');

    const nestedProperties = propertySchema?.properties || {};
    const hasMultipleProperties = Object.keys(value).length > 3;

    // For complex objects with many properties, use nested accordion
    if (hasMultipleProperties) {
        const nestedAccordionId = `nested-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const nestedAccordion = createSectionAccordion(nestedAccordionId, label, value, propertySchema, false);
        return nestedAccordion;
    }

    // For simple objects, render inline
    const labelElement = document.createElement('strong');
    labelElement.classList.add('field-label');
    labelElement.textContent = `${label}:`;
    wrapper.appendChild(labelElement);

    const nestedContainer = document.createElement('div');
    nestedContainer.classList.add('field-nested');

    // Recursively render nested properties
    for (const [nestedKey, nestedValue] of Object.entries(value)) {
        const nestedPropertySchema = nestedProperties[nestedKey];
        const nestedLabel = nestedPropertySchema?.['x-title'] || nestedKey;
        const fieldElement = renderField(nestedValue, nestedLabel, nestedPropertySchema);
        
        if (fieldElement) {
            nestedContainer.appendChild(fieldElement);
        }
    }

    wrapper.appendChild(nestedContainer);
    return wrapper;
}

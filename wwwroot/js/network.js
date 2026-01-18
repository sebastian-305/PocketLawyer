const DEBUG = false;

export function startAnalysisSocket(file, contractType, onMessage) {
    const protocol = window.location.protocol === 'https:' ? 'wss' : 'ws';
    const socket = new WebSocket(`${protocol}://${window.location.host}/${contractType}`);
    socket.addEventListener('open', async () => {
        const buffer = await file.arrayBuffer();
        socket.send(
            JSON.stringify({
                fileName: file.name,
                data: bufferToBase64(buffer),
            }),
        );
    });
    socket.addEventListener('message', (e) => {
        try {
            if (DEBUG) console.log(e.data);
            onMessage(JSON.parse(e.data));
        } catch (err) {
            if (DEBUG) console.error('Ungültige Serverantwort', e.data);
        }
    });
    return socket;
}

function bufferToBase64(buffer) {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

export async function requestMockAnalysis(contractType = 'employment') {
    const endpoint = contractType === 'rental' ? '/rental-mockdata' : '/employment-mockdata';
    const response = await fetch(endpoint, { method: 'GET' });
    const text = await response.text();
    try {
        return JSON.parse(text);
    } catch (err) {
        throw new Error(text || 'Ungültige Serverantwort');
    }
}

export function mockPreValidationMessage() {
    return {
        isReadable: true,
        isContract: true,
        result:
            'Mock Request. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.',
    };
}

// Schema cache to avoid repeated requests
const schemaCache = new Map();

/**
 * Fetches a JSON schema for a specific contract analysis step.
 * Schemas are cached in memory to avoid repeated requests.
 * 
 * @param {string} contractType - Type of contract (e.g., "Rental", "Employment")
 * @param {string} tier - Analysis tier ("Basic" or "Premium")
 * @param {string} step - Analysis step name (e.g., "General", "Costs")
 * @returns {Promise<object|null>} The JSON schema object with x-title and x-styling metadata, or null if not found
 */
export async function fetchSchema(contractType, tier, step) {
    // Create cache key
    const cacheKey = `${contractType}_${tier}_${step}`;
    
    // Check cache first
    if (schemaCache.has(cacheKey)) {
        if (DEBUG) console.log(`Schema cache hit: ${cacheKey}`);
        return schemaCache.get(cacheKey);
    }
    
    // Fetch from API
    try {
        const response = await fetch(`/api/schemas/${contractType}/${tier}/${step}`);
        
        if (response.status === 404) {
            if (DEBUG) console.warn(`Schema not found: ${cacheKey}`);
            // Cache null to avoid repeated 404 requests
            schemaCache.set(cacheKey, null);
            return null;
        }
        
        if (!response.ok) {
            throw new Error(`Failed to fetch schema: ${response.status} ${response.statusText}`);
        }
        
        const schema = await response.json();
        
        // Cache the schema
        schemaCache.set(cacheKey, schema);
        
        if (DEBUG) console.log(`Schema fetched and cached: ${cacheKey}`);
        
        return schema;
    } catch (error) {
        if (DEBUG) console.error(`Error fetching schema ${cacheKey}:`, error);
        // Don't cache errors, allow retry on next call
        return null;
    }
}

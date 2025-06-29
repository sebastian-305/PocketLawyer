const DEBUG = false;

// Enthält Funktionen zur Kommunikation mit dem Backend.

// Baut eine WebSocket-Verbindung auf, überträgt die Datei und liefert eingehende Nachrichten.
export function startAnalysisSocket(file, onMessage) {
    // Protokoll abhängig von HTTP/HTTPS wählen und Verbindung herstellen
    const protocol = window.location.protocol === 'https:' ? 'wss' : 'ws';
    const socket = new WebSocket(`${protocol}://${window.location.host}/analysis`);
    // Nach Verbindungsaufbau Datei lesen und senden
    socket.addEventListener('open', async () => {
        const buffer = await file.arrayBuffer();
        socket.send(
            JSON.stringify({
                fileName: file.name,
                data: bufferToBase64(buffer), // Dateiinhalt Base64-kodiert
            }),
        );
    });
    // Jede eingehende Nachricht an den Callback weiterreichen
    socket.addEventListener('message', (e) => {
        try {
            onMessage(JSON.parse(e.data));
        } catch (err) {
            if (DEBUG) console.error('Ungültige Serverantwort', e.data);
        }
    });
    return socket;
}

// Wandelt einen ArrayBuffer in eine Base64-Zeichenkette um.
function bufferToBase64(buffer) {
    // Browserfreundliche Umwandlung in einen Base64-String
    let binary = '';
    const bytes = new Uint8Array(buffer);
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

// Holt eine zufällige Analyseantwort vom Server.
export async function requestMockAnalysis() {
    // Ruft zufällige Beispielergebnisse vom Server ab
    const response = await fetch('/mockdata', { method: 'GET' });
    const text = await response.text();
    try {
        return JSON.parse(text);
    } catch (err) {
        throw new Error(text || 'Ungültige Serverantwort');
    }
}

// Liefert eine statische Voranalyse für den Demo-Modus.
export function mockPreValidationMessage() {
    // Statische Voranalyse für den Demo-Modus
    return {
        isReadable: true,
        isContract: true,
        result:
            'Mock Request. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.',
    };
}

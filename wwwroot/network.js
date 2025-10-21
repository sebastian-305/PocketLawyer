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

export async function requestMockAnalysis() {
    const response = await fetch('/employment-mockdata', { method: 'GET' });
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

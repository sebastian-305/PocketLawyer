// Erstellt die Fetch-Requests für Voranalyse und Hauptanalyse.
// Gibt die gewählte Datei sowie ein Flag zurück, ob wir im Mock-Modus arbeiten
// (keine echte Datei ausgewählt).
export function prepareRequests(fileInput) {
    let validationRequest;
    let file = null;
    let mock = false;

    if (fileInput.files.length > 0) {
        console.debug("REAL REQUEST!");
        const formData = new FormData();
        file = fileInput.files[0];
        formData.append("myfile", file);
        validationRequest = new Request("/preanalysis", { method: "POST", body: formData });
    } else {
        console.debug("MOCK REQUEST!");
        mock = true;
    }

    return { validationRequest, file, mock };
}

// Baut den Request für den Analyse-Endpunkt
export function createAnalysisRequest(file, kvName, mock) {
    if (mock) {
        return new Request("/mockdata", { method: "GET" });
    }
    const formData = new FormData();
    formData.append("myfile", file);
    if (kvName) {
        formData.append("kollektivvertrag", kvName);
    }
    return new Request("/mainanalysis", { method: "POST", body: formData });
}

// Ruft den Voranalyse-Endpunkt auf und liest die JSON-Antwort ein.
// Im Mock-Modus wird stattdessen ein statisches Objekt zurückgegeben.
export async function requestPreValidation(validationRequest, mock) {
    if (mock) {
        return {
            isReadable: true,
            isContract: true,
            result:
                "Mock Request. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.",
        };
    }

    const response = await fetch(validationRequest);
    document.getElementById("printbtn").style.visibility = "hidden";
    let messageText = await response.text();
    let message;
    try {
        message = JSON.parse(messageText);
    } catch (err) {
        throw new Error(messageText || "Ungültige Serverantwort");
    }
    if (message.error) {
        throw new Error(message.error);
    }
    return message;
}

// Ruft den Analyse-Endpunkt auf und liest die JSON-Antwort ein
export async function requestContractAnalysis(analyzeRequest) {
    const response = await fetch(analyzeRequest);
    let messageText = await response.text();
    let message;
    try {
        message = JSON.parse(messageText);
    } catch (err) {
        throw new Error(messageText || "Ungültige Serverantwort");
    }
    return message;
}

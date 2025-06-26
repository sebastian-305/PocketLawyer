// Bereitet die HTTP-Requests vor, abhängig davon ob eine Datei ausgewählt wurde
export function prepareRequests(fileInput) {
    let validationRequest;
    let analyzeRequest;
    let mock = false;

    if (fileInput.files.length > 0) {
        // Es wurde eine Datei hochgeladen -> echte Anfrage an den Server
        console.debug("REAL REQUEST!");
        const formData = new FormData();
        formData.append("myfile", fileInput.files[0]);
        validationRequest = new Request("/validate", { method: "POST", body: formData });
        analyzeRequest = new Request("/contract", { method: "POST", body: formData });
    } else {
        // Keine Datei: es wird nur ein Mock zurückgegeben
        console.debug("MOCK REQUEST!");
        mock = true;
        analyzeRequest = new Request("/mockdata", { method: "GET" });
    }

    // Dem Aufrufer werden beide Requests und ein Flag zurückgegeben
    return { validationRequest, analyzeRequest, mock };
}

// Sendet die Vorvalidierung und liefert das Ergebnis als Objekt zurück
export async function requestPreValidation(validationRequest, mock) {
    if (mock) {
        // Für Demozwecke geben wir hier feste Daten zurück
        return {
            isReadable: true,
            isContract: true,
            result:
                "Mock Request. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.",
        };
    }

    // Anfrage an den Server senden
    const response = await fetch(validationRequest);
    // Drucken deaktivieren, bis die Analyse abgeschlossen ist
    document.getElementById("printbtn").style.visibility = "hidden";
    let messageText = await response.text();
    let message;
    try {
        message = JSON.parse(messageText);
    } catch (err) {
        throw new Error(messageText || "Ungültige Serverantwort");
    }
    // Server kann Fehlermeldungen zurückliefern
    if (message.error) {
        throw new Error(message.error);
    }
    // Parsed JSON an den Aufrufer weitergeben
    return message;
}

// Sendet den eigentlichen Analyse-Request an den Server
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

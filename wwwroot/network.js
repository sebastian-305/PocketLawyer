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

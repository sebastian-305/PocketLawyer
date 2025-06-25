export function prepareRequests(fileInput) {
    let validationRequest;
    let analyzeRequest;
    let mock = false;

    if (fileInput.files.length > 0) {
        console.info("REAL REQUEST!");
        const formData = new FormData();
        formData.append("myfile", fileInput.files[0]);
        validationRequest = new Request("/validate", { method: "POST", body: formData });
        analyzeRequest = new Request("/contract", { method: "POST", body: formData });
    } else {
        console.warn("MOCK REQUEST!");
        mock = true;
        analyzeRequest = new Request("/mockdata", { method: "GET" });
    }

    return { validationRequest, analyzeRequest, mock };
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

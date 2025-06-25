function printer() {
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
    doc.head.appendChild(link);

    const content = document.getElementById("evaluation-body").cloneNode(true);
    doc.body.appendChild(content);

    printWindow.focus();
    printWindow.print();
    printWindow.close();
}
function renderClause({ title, textLabel, textValue, extraFields = [] }) {
    const container = document.createElement("div");
    container.classList.add("item");

    const heading = document.createElement("strong");
    heading.textContent = title;
    container.appendChild(heading);

    const firstWrap = document.createElement("p");
    const firstTit = document.createElement("em");
    firstTit.textContent = textLabel;
    const firstVal = document.createElement("div");
    firstVal.textContent = textValue;
    firstWrap.appendChild(firstTit);
    firstWrap.appendChild(firstVal);
    container.appendChild(firstWrap);

    extraFields.forEach((f) => {
        const wrap = document.createElement("p");
        const titleEl = document.createElement(f.emphasize ? "em" : "strong");
        titleEl.textContent = f.label;
        const valueEl = document.createElement("div");
        valueEl.textContent = f.value;
        wrap.appendChild(titleEl);
        wrap.appendChild(valueEl);
        container.appendChild(wrap);
    });

    return container;
}
function generateClauses(clause) {
    return renderClause({
        title: clause.titel,
        textLabel: "Zitat:",
        textValue: clause.zitat,
        extraFields: [
            { label: "Problem:", value: clause.problem },
            { label: "Rechtsgrundlage:", value: clause.rechtsgrundlage },
        ],
    });
}
function generateGoodClauses(clause) {
    return renderClause({
        title: clause.titel,
        textLabel: "Zitat:",
        textValue: clause.zitat,
        extraFields: [{ label: "Vorteil:", value: clause.vorteil }],
    });
}
function generateConflictClauses(clause) {
    return renderClause({
        title: clause.titel,
        textLabel: "Zitat A:",
        textValue: clause.zitat_klausel_a,
        extraFields: [
            { label: "Zitat B:", value: clause.zitat_klausel_b, emphasize: true },
            { label: "Problem:", value: clause.problem },
            { label: "Rechtsgrundlage:", value: clause.rechtsgrundlage },
        ],
    });
}

function createLoadingElement() {
    const loading = document.createElement("img");
    loading.setAttribute("alt", "loading");
    loading.setAttribute("src", "Media/loading.gif");
    loading.style.width = "30%";
    loading.style.margin = "auto";
    loading.style.display = "block";
    return loading;
}

function prepareRequests(fileInput) {
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

async function requestPreValidation(validationRequest, mock) {
    if (mock) {
        return { isReadable: true, isContract: true, result: "Mock Request. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet." };
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

async function requestContractAnalysis(analyzeRequest) {
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

function renderPreValidation(message, body) {
    body.replaceChildren(generatePreAnalysis(message));
}

function renderContractResult(message, body) {
    body.appendChild(generateAnalysis(message));
}

function displayUploadError(ex, body, loading, preAnalysisShown) {
    console.error(ex);
    if (body.contains(loading)) {
        body.removeChild(loading);
    }
    if (!preAnalysisShown) {
        body.replaceChildren();
    }
    const err = document.createElement("div");
    err.classList.add("error-message");
    err.textContent = `Fehler: ${ex.message}`;
    body.appendChild(err);
}

async function upload_file(event) {
    event.preventDefault();

    console.info("File upload started.");

    const body = document.getElementById("evaluation-body");
    const loading = createLoadingElement();
    body.replaceChildren(loading);

    const { validationRequest, analyzeRequest, mock } = prepareRequests(event.target.myfile);

    let preAnalysisShown = false;

    try {
        const validationMessage = await requestPreValidation(validationRequest, mock);
        renderPreValidation(validationMessage, body);
        preAnalysisShown = true;

        if (!validationMessage.isReadable || !validationMessage.isContract) {
            throw new Error(`isReadable: ${validationMessage.isReadable}, isContract: ${validationMessage.isContract}`);
        }

        body.appendChild(loading);
        const analysisMessage = await requestContractAnalysis(analyzeRequest);
        body.removeChild(loading);
        renderContractResult(analysisMessage, body);
        document.getElementById("printbtn").style.visibility = "visible";
    } catch (ex) {
        displayUploadError(ex, body, loading, preAnalysisShown);
    } finally {
        event.target.myfile.value = null;
    }
}
function generatePreAnalysis(message) {
    const container = document.createElement("div");
    container.classList.add("preanalysis-container");
    if (!message.isReadable) {
        const heading = document.createElement("h2");
        heading.innerHTML =
            "Vertragsdatei ist nicht lesbar. Falls das Problem weiterhin besteht, kopieren sie den Inhalt in eine .txt Datei und analysieren Sie diese!";

        container.appendChild(heading);
    } else if (!message.isContract) {
        const heading = document.createElement("h2");
        heading.innerHTML =
            "Vertragsdatei ist kein Arbeitsvertrag. Versichern Sie sich, dass es sich um einen Arbeitsvertrag handelt und versuchen Sie es erneut.";

        container.appendChild(heading);
    }

    const fadWrap = document.createElement("div");
    fadWrap.classList.add("section", "summary-box");
    const fadHd = document.createElement("h3");
    fadHd.innerHTML = "Voranalyse";
    fadWrap.appendChild(fadHd);
    const fadVal = document.createElement("p");
    console.debug("Voranalyse:");
    console.debug(message.result);
    fadVal.textContent = message.result;
    fadWrap.appendChild(fadVal);
    container.appendChild(fadWrap);
    return container;
}
function generateAnalysis(message) {
    const container = document.createElement("div");
    container.classList.add("analysis-container");

    const hdr = document.createElement("div");
    hdr.classList.add("header-a");

    const heading = document.createElement("h2");
    heading.innerHTML = "Deine Vertragsanalyse";
    hdr.appendChild(heading);
    container.appendChild(hdr);

    container.appendChild(generateSummary(message.summary));

    const badWrap = document.createElement("div");
    badWrap.classList.add("section", "bad");
    const badHd = document.createElement("h3");
    badHd.innerHTML = "❗ Unzulässige Klauseln";
    badWrap.appendChild(badHd);
    message.unzulaessige_klauseln.forEach((e) =>
        badWrap.appendChild(generateClauses(e)),
    );
    if (
        message["unzulaessige_klauseln"] !== undefined &&
        message.unzulaessige_klauseln != null &&
        message.unzulaessige_klauseln.length > 0
    )
        container.appendChild(badWrap);

    const nadWrap = document.createElement("div");
    nadWrap.classList.add("section", "warning");
    const nadHd = document.createElement("h3");
    nadHd.innerHTML = "⚠️ Nachteilige Klauseln";
    nadWrap.appendChild(nadHd);
    message.nachteile_klauseln.forEach((e) =>
        nadWrap.appendChild(generateClauses(e)),
    );
    if (
        message["nachteile_klauseln"] !== undefined &&
        message.nachteile_klauseln != null &&
        message.nachteile_klauseln.length > 0
    )
        container.appendChild(nadWrap);

    const cadWrap = document.createElement("div");
    cadWrap.classList.add("section", "conflict");
    const cadHd = document.createElement("h3");
    cadHd.innerHTML = "🟠 Widersprüchliche Klauseln";
    cadWrap.appendChild(cadHd);
    message.widerspruechliche_klauseln.forEach((e) =>
        cadWrap.appendChild(generateConflictClauses(e)),
    );
    if (
        message["widerspruechliche_klauseln"] !== undefined &&
        message.widerspruechliche_klauseln != null &&
        message.widerspruechliche_klauseln.length > 0
    )
        container.appendChild(cadWrap);

    const gadWrap = document.createElement("div");
    gadWrap.classList.add("section", "good");
    const gadHd = document.createElement("h3");
    gadHd.innerHTML = "✅ Vorteilhafte Klauseln";
    gadWrap.appendChild(gadHd);
    message.vorteilhafte_klauseln.forEach((e) =>
        gadWrap.appendChild(generateGoodClauses(e)),
    );
    if (
        message["vorteilhafte_klauseln"] !== undefined &&
        message.vorteilhafte_klauseln != null &&
        message.vorteilhafte_klauseln.length > 0
    )
        container.appendChild(gadWrap);

    const fadWrap = document.createElement("div");
    fadWrap.classList.add("section", "summary-box");
    const fadHd = document.createElement("h3");
    fadHd.innerHTML = "📝 Fazit";
    fadWrap.appendChild(fadHd);
    const fadVal = document.createElement("p");
    console.debug("Fazit:");
    console.debug(message.fazit);
    fadVal.textContent = message.fazit;
    fadWrap.appendChild(fadVal);
    container.appendChild(fadWrap);

    return container;
    function generateSummary(summaryObj) {
        const container = document.createElement("div");

        container.classList.add("section", "summary");
        const heading = document.createElement("h3");

        heading.innerHTML = "Zusammenfassung";

        container.appendChild(heading);

        const list = document.createElement("ul");

        container.appendChild(list);
        const keys = [
            "Arbeitgeber",
            "Tätigkeit",
            "Beginn",
            "Dauer",
            "Gehalt (Brutto)",
            "Zuschläge",
            "Arbeitsstunden",
            "Überstunden",
            "Probezeit",
        ];
        const values = [
            summaryObj.arbeitgeber,
            summaryObj.taetigkeit,
            summaryObj.beginn,
            summaryObj.dauer,
            summaryObj.gehalt_brutto,
            summaryObj.zuschlaege,
            summaryObj.arbeitsstunden,
            summaryObj.ueberstunden,
            summaryObj.probezeit,
        ];

        for (let i = 0; i < keys.length; ++i) {
            let elemWrapper = document.createElement("li");
            let elemKey = document.createElement("strong");
            let elemValue = document.createElement("div");

            elemWrapper.appendChild(elemKey);
            elemWrapper.appendChild(elemValue);
            elemKey.textContent = keys[i] + ":";
            elemValue.textContent = values[i];
            list.appendChild(elemWrapper);
        }

        return container;
    }
}

document.addEventListener("DOMContentLoaded", () => {
    document
        .querySelectorAll(".upload-form")
        .forEach((form) => form.addEventListener("submit", upload_file));
});

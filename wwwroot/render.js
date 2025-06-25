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

export function createLoadingElement() {
    const loading = document.createElement("img");
    loading.setAttribute("alt", "loading");
    loading.setAttribute("src", "Media/loading.gif");
    loading.style.width = "30%";
    loading.style.margin = "auto";
    loading.style.display = "block";
    return loading;
}

export function renderPreValidation(message, body) {
    body.replaceChildren(generatePreAnalysis(message));
}

export function renderContractResult(message, body) {
    body.appendChild(generateAnalysis(message));
}

export function displayUploadError(ex, body, loading, preAnalysisShown) {
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

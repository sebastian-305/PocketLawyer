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

function formatRechtsgrundlage(arr) {
    if (!Array.isArray(arr)) return arr ?? "";
    return arr
        .map((e) => `${e.rechtsgrundlage}: ${e.wortlaut}`)
        .join("; ");
}

function generateClauses(clause) {
    return renderClause({
        title: clause.titel,
        textLabel: "Zitat:",
        textValue: clause.zitat,
        extraFields: [
            { label: "Problem:", value: clause.problem },
            {
                label: "Rechtsgrundlage:",
                value: formatRechtsgrundlage(clause.rechtsgrundlage),
            },
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
            {
                label: "Rechtsgrundlage:",
                value: formatRechtsgrundlage(clause.rechtsgrundlage),
            },
        ],
    });
}

function createAccordionItem(title, content, cls, id) {
    const item = document.createElement("div");
    item.classList.add("accordion-item", cls);

    const header = document.createElement("h2");
    header.classList.add("accordion-header");

    const button = document.createElement("button");
    button.classList.add("accordion-button");
    button.type = "button";
    button.setAttribute("data-bs-toggle", "collapse");
    button.setAttribute("data-bs-target", `#${id}`);
    button.setAttribute("aria-expanded", "true");
    button.setAttribute("aria-controls", id);
    button.innerHTML = title;

    header.appendChild(button);
    item.appendChild(header);

    const collapse = document.createElement("div");
    collapse.id = id;
    collapse.classList.add("accordion-collapse", "collapse", "show");

    const body = document.createElement("div");
    body.classList.add("accordion-body", "section", cls);
    body.appendChild(content);

    collapse.appendChild(body);
    item.appendChild(collapse);

    return item;
}

export function createLoadingElement() {
    const loading = document.createElement("div");
    loading.classList.add("loading-spinner");
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
    err.setAttribute("role", "alert");
    err.setAttribute("tabindex", "-1");
    err.textContent = `Fehler: ${ex.message}`;
    body.appendChild(err);
    err.focus();
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


    let idx = 0;
    container.appendChild(
        createAccordionItem(
            "Zusammenfassung",
            generateSummary(message.summary),
            "summary",
            `acc-${idx++}`
        )
    );

    if (
        message["unzulaessige_klauseln"] !== undefined &&
        message.unzulaessige_klauseln != null &&
        message.unzulaessige_klauseln.length > 0
    ) {
        const wrap = document.createElement("div");
        message.unzulaessige_klauseln.forEach((e) =>
            wrap.appendChild(generateClauses(e)),
        );
        container.appendChild(
            createAccordionItem(
                "❗ Unzulässige Klauseln",
                wrap,
                "bad",
                `acc-${idx++}`
            )
        );
    }

    if (
        message["nachteile_klauseln"] !== undefined &&
        message.nachteile_klauseln != null &&
        message.nachteile_klauseln.length > 0
    ) {
        const wrap = document.createElement("div");
        message.nachteile_klauseln.forEach((e) =>
            wrap.appendChild(generateClauses(e)),
        );
        container.appendChild(
            createAccordionItem(
                "⚠️ Nachteilige Klauseln",
                wrap,
                "warning",
                `acc-${idx++}`
            )
        );
    }

    if (
        message["widerspruechliche_klauseln"] !== undefined &&
        message.widerspruechliche_klauseln != null &&
        message.widerspruechliche_klauseln.length > 0
    ) {
        const wrap = document.createElement("div");
        message.widerspruechliche_klauseln.forEach((e) =>
            wrap.appendChild(generateConflictClauses(e)),
        );
        container.appendChild(
            createAccordionItem(
                "🟠 Widersprüchliche Klauseln",
                wrap,
                "conflict",
                `acc-${idx++}`
            )
        );
    }

    if (
        message["vorteilhafte_klauseln"] !== undefined &&
        message.vorteilhafte_klauseln != null &&
        message.vorteilhafte_klauseln.length > 0
    ) {
        const wrap = document.createElement("div");
        message.vorteilhafte_klauseln.forEach((e) =>
            wrap.appendChild(generateGoodClauses(e)),
        );
        container.appendChild(
            createAccordionItem(
                "✅ Vorteilhafte Klauseln",
                wrap,
                "good",
                `acc-${idx++}`
            )
        );
    }

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

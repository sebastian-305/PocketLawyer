const DEBUG = true;

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

export function createLoadingElement() {
    const loading = document.createElement("div");
    loading.classList.add("loading-spinner");
    loading.setAttribute("role", "status");
    const srText = document.createElement("span");
    srText.classList.add("visually-hidden");
    srText.textContent = "Analyse läuft…";
    loading.appendChild(srText);
    return loading;
}

export function displayUploadError(ex, body, loading, preAnalysisShown) {
    if (DEBUG) console.error(ex);
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

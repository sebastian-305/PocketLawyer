async function uploadFile(event) {
    // Handles both submit and click triggers
    if (event) event.preventDefault();
    const body = document.getElementById("evaluation-body");
    if (!body) return;
    const loading = createLoadingElement();
    body.replaceChildren(loading);
}

document.addEventListener("DOMContentLoaded", () => {
    // 1) Buttons/Links, die das Evaluation-Modal öffnen
    document
        .querySelectorAll('[data-bs-target="#evaluation"]')
        .forEach((el) => el.addEventListener("click", uploadFile));

    // 2) Falls doch ein Submit-Button verwendet wird
    document
        .querySelectorAll(".upload-form")
        .forEach((form) => form.addEventListener("submit", uploadFile));
});

export function createLoadingElement() {
    const loading = document.createElement("div");
    const srText = document.createElement("span");
    srText.style.color = "black";
    srText.textContent =
        "Die Anmeldung zur kostenfreien Testanalyse ist demnächst verfügbar!";
    loading.appendChild(srText);
    return loading;
}

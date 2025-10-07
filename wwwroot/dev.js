async function uploadFile(event) {
    event.preventDefault();
    const body = document.getElementById('evaluation-body');
    const loading = createLoadingElement();
    body.replaceChildren(loading);
}

document.addEventListener('DOMContentLoaded', () => {
    document
        .querySelectorAll('.upload-form')
        .forEach((form) => form.addEventListener('submit', uploadFile));
});

export function createLoadingElement() {
    const loading = document.createElement("div");
    const srText = document.createElement("span");
    srText.style.color = "black";
    srText.textContent = "Die Anmeldung zur kostenfreien Testanalyse ist demnächst verfügbar!";
    loading.appendChild(srText);
    return loading;
}

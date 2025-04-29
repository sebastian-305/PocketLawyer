







function generateSummary(summaryObj){

    const container = document.createElement("div");

    const keys = ["Arbeitgeber", "Tätigkeit", "Beginn", "Dauer", "Gehalt (Brutto)", "Zuschläge", "Arbeitsstunden", "Überstunden", "Probezeit"];
    const values = [{ arbeitgeber, taeltigkeit, beginn, dauer, gehalt_brutto, zuschlaege, arbeitsstunden, ueberstunden, probezeit }];



    for (let i = 0; i < keys.length; ++i) {
        let elemWrapper = document.createElement("div");
        let elemKey = document.createElement("div");
        let elemValue = document.createElement("div");

        elemWrapper.appendChild(elemKey);
        elemWrapper.appendChild(elemValue);
        elemKey = keys[i];
        elemValue = values[i];
        container.appendChild(elemWrapper);
    }

    return container;
}

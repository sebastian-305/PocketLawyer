function printer() {
    var WinPrint = window.open('', 'Vertragsanalyse', 'left=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0');

    const printelem = document.getElementById("evaluation-body").cloneNode(true);
    //printelem.classList.add("printerclass");
    //printelem.removeChild(document.getElementById("closex"));
    //printelem.removeChild(document.getElementById("closebtn"));
    //printelem.removeChild(document.getElementById("printbtn"));

    WinPrint.document.write(printelem.innerHTML);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    WinPrint.close();
}
function upload_file(event) {
    event.preventDefault();

    console.log("File Upload started.")

    const loading = document.createElement("img");
    loading.setAttribute('alt', 'loading');
    loading.setAttribute('src', 'media/loading.gif');
    loading.style.width = "30%";
    loading.style.margin = "auto";
    loading.style.display = "block";

    document.getElementById("evaluation-body").replaceChildren(
        loading);

    if (event.target.myfile.files.length > 0) {
        console.log("REAL REQUEST!");
        const init = {
            method: "POST",
            headers: {
                //'Content-Type': 'application/x-binary',
                'filename': event.target.myfile.files[0].name,
            },
            body: event.target.myfile.files[0]
        };

        validationRequest = new Request("/validate", init);
        analyzeRequest = new Request("/contract", init);
    }
    else {
        console.warn("MOCK REQUEST!");
        request = new Request("/mockdata", {
            method: "GET",
        });
    }

    fetch(validationRequest)
        .then(response => {
            document.getElementById("printbtn").style.visibility = "hidden";
            let status = response.status
            console.log("Response:");
            console.log(status);
            console.log(response);
            return response.json();
        })
        .then(message => {
            console.log(message);
            document.getElementById("evaluation-body").replaceChildren(
                generatePreAnalysis(message));

            if (message.isReadable && message.isContract) {
                document.getElementById("evaluation-body").appendChild(loading);
                return fetch(analyzeRequest);
            }
            else
                throw new Error(`isReadable: ${message.isReadable}, isContract: ${message.isContract}`);
        }).then(response => {
            let status = response.status
            console.log("Response:");
            console.log(status);

            event.target.myfile.value = null;

            return response.json();
        }).then(message => {
            console.log(message);
            document.getElementById("evaluation-body").removeChild(loading);
            document.getElementById("evaluation-body")
                .appendChild(generateAnalysis(message));
            document.getElementById("printbtn").style.visibility = "visible";
        }).catch(ex => {
            console.warn(ex);
        });

}

function generatePreAnalysis(message) {
    const container = document.createElement("div");
    container.classList.add("preanalysis-container");
    if (!message.isReadable) {

        const heading = document.createElement("h2");
        heading.innerHTML = "Vertragsdatei ist nicht lesbar. Falls das Problem weiterhin besteht, kopieren sie den Inhalt in eine .txt Datei und analysieren Sie diese!";

        container.appendChild(heading);
    } else if (!message.isContract) {

        const heading = document.createElement("h2");
        heading.innerHTML = "Vertragsdatei ist kein Arbeitsvertrag. Versichern Sie sich, dass es sich um einen Arbeitsvertrag handelt und versuchen Sie es erneut.";

        container.appendChild(heading);
    }

    const fadWrap = document.createElement("div");
    fadWrap.classList.add("section", "summary-box");
    const fadHd = document.createElement("h3");
    fadHd.innerHTML = "Voranalyse";
    fadWrap.appendChild(fadHd);
    const fadVal = document.createElement("p");
    console.log("Voranalyse:");
    console.log(message.result);
    fadVal.innerHTML = message.result;
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
    message.unzulaessige_klauseln.forEach((e) => badWrap.appendChild(generateClauses(e)));
    if (message["unzulaessige_klauseln"] !== undefined && message.unzulaessige_klauseln != null && message.unzulaessige_klauseln.length > 0)
        container.appendChild(badWrap);

    const nadWrap = document.createElement("div");
    nadWrap.classList.add("section", "warning");
    const nadHd = document.createElement("h3");
    nadHd.innerHTML = "⚠️ Nachteilige Klauseln";
    nadWrap.appendChild(nadHd);
    message.nachteile_klauseln.forEach((e) => nadWrap.appendChild(generateClauses(e)));
    if (message["nachteile_klauseln"] !== undefined && message.nachteile_klauseln != null && message.nachteile_klauseln.length > 0)
        container.appendChild(nadWrap);

    const cadWrap = document.createElement("div");
    cadWrap.classList.add("section", "conflict");
    const cadHd = document.createElement("h3");
    cadHd.innerHTML = "🟠 Widersprüchliche Klauseln";
    cadWrap.appendChild(cadHd);
    message.widerspruechliche_klauseln.forEach((e) => cadWrap.appendChild(generateConflictClauses(e)));
    if (message["widerspruechliche_klauseln"] !== undefined && message.widerspruechliche_klauseln != null && message.widerspruechliche_klauseln.length > 0)
        container.appendChild(cadWrap);

    const gadWrap = document.createElement("div");
    gadWrap.classList.add("section", "good");
    const gadHd = document.createElement("h3");
    gadHd.innerHTML = "✅ Vorteilhafte Klauseln";
    gadWrap.appendChild(gadHd);
    message.vorteilhafte_klauseln.forEach((e) => gadWrap.appendChild(generateGoodClauses(e)));
    if (message["vorteilhafte_klauseln"] !== undefined && message.vorteilhafte_klauseln != null && message.vorteilhafte_klauseln.length > 0)
        container.appendChild(gadWrap);


    const fadWrap = document.createElement("div");
    fadWrap.classList.add("section", "summary-box");
    const fadHd = document.createElement("h3");
    fadHd.innerHTML = "📝 Fazit";
    fadWrap.appendChild(fadHd);
    const fadVal = document.createElement("p");
    console.log("Fazit:");
    console.log(message.fazit);
    fadVal.innerHTML = message.fazit;
    fadWrap.appendChild(fadVal);
    container.appendChild(fadWrap);

    return container;
}
function generateSummary(summaryObj) {

    const container = document.createElement("div");

    container.classList.add("section", "summary");
    const heading = document.createElement("h3");

    heading.innerHTML = "Zusammenfassung";

    container.appendChild(heading);

    const list = document.createElement("ul");

    container.appendChild(list);
    const options = { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' };
    const keys = ["Arbeitgeber", "Tätigkeit", "Beginn", "Dauer", "Gehalt (Brutto)", "Zuschläge", "Arbeitsstunden", "Überstunden", "Probezeit"];
    const values = [summaryObj.arbeitgeber, summaryObj.taetigkeit, summaryObj.beginn, summaryObj.dauer, summaryObj.gehalt_brutto, summaryObj.zuschlaege, summaryObj.arbeitsstunden, summaryObj.ueberstunden, summaryObj.probezeit];

    for (let i = 0; i < keys.length; ++i) {
        let elemWrapper = document.createElement("li");
        let elemKey = document.createElement("strong");
        let elemValue = document.createElement("div");

        elemWrapper.appendChild(elemKey);
        elemWrapper.appendChild(elemValue);
        elemKey.innerHTML = keys[i] + ":";
        elemValue.innerHTML = values[i];
        list.appendChild(elemWrapper);
    }

    return container;
}
function generateClauses(clause) {
    const container = document.createElement("div");
    container.classList.add("item");

    const heading = document.createElement("strong");
    heading.innerHTML = clause.titel
    container.appendChild(heading);

    const zitWrap = document.createElement("p");
    const zitTit = document.createElement("em");
    zitTit.innerHTML = "Zitat:";
    const zitVal = document.createElement("div");
    zitVal.innerHTML = clause.zitat;
    zitWrap.appendChild(zitTit);
    zitWrap.appendChild(zitVal);
    container.appendChild(zitWrap);

    const probWrap = document.createElement("p");
    const probTit = document.createElement("strong");
    probTit.innerHTML = "Problem:";
    const probVal = document.createElement("div");
    probVal.innerHTML = clause.problem;
    probWrap.appendChild(probTit);
    probWrap.appendChild(probVal);
    container.appendChild(probWrap);


    const robWrap = document.createElement("p");
    const robTit = document.createElement("strong");
    robTit.innerHTML = "Rechtsgrundlage:";
    const robVal = document.createElement("div");
    robVal.innerHTML = clause.rechtsgrundlage;
    robWrap.appendChild(robTit);
    robWrap.appendChild(robVal);
    container.appendChild(robWrap);
    return container;
}
function generateGoodClauses(clause) {
    const container = document.createElement("div");
    container.classList.add("item");

    const heading = document.createElement("strong");
    heading.innerHTML = clause.titel
    container.appendChild(heading);

    const zitWrap = document.createElement("p");
    const zitTit = document.createElement("em");
    zitTit.innerHTML = "Zitat:";
    const zitVal = document.createElement("div");
    zitVal.innerHTML = clause.zitat;
    zitWrap.appendChild(zitTit);
    zitWrap.appendChild(zitVal);
    container.appendChild(zitWrap);

    const probWrap = document.createElement("p");
    const probTit = document.createElement("strong");
    probTit.innerHTML = "Vorteil:";
    const probVal = document.createElement("div");
    probVal.innerHTML = clause.vorteil;
    probWrap.appendChild(probTit);
    probWrap.appendChild(probVal);
    container.appendChild(probWrap);



    return container;
}
function generateConflictClauses(clause) {
    const container = document.createElement("div");
    container.classList.add("item");

    const heading = document.createElement("strong");
    heading.innerHTML = clause.titel
    container.appendChild(heading);

    const zitWrap = document.createElement("p");
    const zitTit = document.createElement("em");
    zitTit.innerHTML = "Zitat A:";
    const zitVal = document.createElement("div");
    zitVal.innerHTML = clause.zitat_klausel_a;
    zitWrap.appendChild(zitTit);
    zitWrap.appendChild(zitVal);
    container.appendChild(zitWrap);

    const zitbWrap = document.createElement("p");
    const zitbTit = document.createElement("em");
    zitbTit.innerHTML = "Zitat B:";
    const zitbVal = document.createElement("div");
    zitbVal.innerHTML = clause.zitat_klausel_b;
    zitbWrap.appendChild(zitbTit);
    zitbWrap.appendChild(zitbVal);
    container.appendChild(zitbWrap);

    const probWrap = document.createElement("p");
    const probTit = document.createElement("strong");
    probTit.innerHTML = "Problem:";
    const probVal = document.createElement("div");
    probVal.innerHTML = clause.problem;
    probWrap.appendChild(probTit);
    probWrap.appendChild(probVal);
    container.appendChild(probWrap);


    const robWrap = document.createElement("p");
    const robTit = document.createElement("strong");
    robTit.innerHTML = "Rechtsgrundlage:";
    const robVal = document.createElement("div");
    robVal.innerHTML = clause.rechtsgrundlage;
    robWrap.appendChild(robTit);
    robWrap.appendChild(robVal);
    container.appendChild(robWrap);
    return container;
}



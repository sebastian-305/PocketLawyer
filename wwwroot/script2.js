//fetch("/ai").then(e => {
//    console.log(e);
//    return e.text();
//})
//    .then(
//        e => {
//            document.body.innerHTML += e;

//            console.log(e);
//        });


function upload_file(event) {
    event.preventDefault();

    console.log("File Upload started.")


    let request;

    if (event.target.myfile.files.length > 0) {
        console.log("REAL REQUEST!");
        request = new Request("/upload", {
            method: "POST",
            headers: {
                //'Content-Type': 'application/x-binary',
                'filename': event.target.myfile.files[0].name,
            },
            body: event.target.myfile.files[0]
        });
    }
    else {
        console.warn("MOCK REQUEST!");
        request = new Request("/mockdata", {
            method: "GET",
            
        });
    }
    



    fetch(request).then(response => {
        let status = response.status
        console.log("Response:");
        console.log(status);

        event.target.myfile.value = null;

        return response.json();
    }).then(message => {
        console.log(message);
        document.body.appendChild(generateAnalysis(message));
    });

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
    container.appendChild(badWrap);

    const nadWrap = document.createElement("div");
    nadWrap.classList.add("section", "warning");
    const nadHd = document.createElement("h3");
    nadHd.innerHTML = "⚠️ Nachteilige Klauseln";
    nadWrap.appendChild(nadHd);
    message.nachteile_klauseln.forEach((e) => nadWrap.appendChild(generateClauses(e)));
    container.appendChild(nadWrap);

    const cadWrap = document.createElement("div");
    cadWrap.classList.add("section", "conflict");
    const cadHd = document.createElement("h3");
    cadHd.innerHTML = "🟠 Widersprüchliche Klauseln";
    cadWrap.appendChild(cadHd);
    message.widerspruechliche_klauseln.forEach((e) => cadWrap.appendChild(generateConflictClauses(e)));
    container.appendChild(cadWrap);

    const gadWrap = document.createElement("div");
    gadWrap.classList.add("section", "good");
    const gadHd = document.createElement("h3");
    gadHd.innerHTML = "✅ Vorteilhafte Klauseln";
    gadWrap.appendChild(gadHd);
    message.vorteilhafte_klauseln.forEach((e) => gadWrap.appendChild(generateGoodClauses(e)));
    container.appendChild(gadWrap);


    const fadWrap = document.createElement("div");
    fadWrap.classList.add("section", "summary-box");
    const fadHd = document.createElement("h3");
    fadHd.innerHTML = "📝 Fazit";
    fadWrap.appendChild(fadHd);
    const fadVal = document.createElement("p");
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
    const values = [summaryObj.arbeitgeber, summaryObj.taeltigkeit, summaryObj.beginn, summaryObj.dauer, summaryObj.gehalt_brutto, summaryObj.zuschlaege, summaryObj.arbeitsstunden, summaryObj.ueberstunden, summaryObj.probezeit];

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



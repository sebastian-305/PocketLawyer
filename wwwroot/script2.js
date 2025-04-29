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

    var request = new Request("/upload", {
        method: 'POST',
        headers: {
            //'Content-Type': 'application/x-binary',
            'filename': event.target.myfile.files[0].name,
        },
        body: event.target.myfile.files[0]
    });



    fetch(request).then(response => {
        let status = response.status
        console.log("Response:");
        console.log(status);

        event.target.myfile.value = null;

        return response.json();
    }).then(message => {
        console.log(message);
        document.body.appendChild(generateSummary(message.summary));
    });

}


function generateSummary(summaryObj) {

    const container = document.createElement("div");

    const heading = document.createElement("h2");

    heading.innerHTML = "Zusammenfassung";

    container.appendChild(heading)

    const keys = ["Arbeitgeber", "Tätigkeit", "Beginn", "Dauer", "Gehalt (Brutto)", "Zuschläge", "Arbeitsstunden", "Überstunden", "Probezeit"];
    const values = [summaryObj.arbeitgeber, summaryObj.taeltigkeit, summaryObj.beginn, summaryObj.dauer, summaryObj.gehalt_brutto, summaryObj.zuschlaege, summaryObj.arbeitsstunden, summaryObj.ueberstunden, summaryObj.probezeit];

    for (let i = 0; i < keys.length; ++i) {
        let elemWrapper = document.createElement("div");
        let elemKey = document.createElement("b");
        let elemValue = document.createElement("div");

        elemWrapper.appendChild(elemKey);
        elemWrapper.appendChild(elemValue);
        elemKey.innerHTML = keys[i];
        elemValue.innerHTML = values[i];
        container.appendChild(elemWrapper);
        console.log(elemWrapper);
    }

    return container;
}


function generateClauses(clause) {
    const container = document.createElement("div");
    let elemWrapper = document.createElement("div");
    let elemKey = document.createElement("b");
    let elemValue = document.createElement("div");

    elemWrapper.appendChild(elemKey);
    elemWrapper.appendChild(elemValue);
    elemKey.innerHTML = keys[i];
    elemValue.innerHTML = values[i];
    container.appendChild(elemWrapper);
    console.log(elemWrapper);

}
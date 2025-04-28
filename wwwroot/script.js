fetch("/ai").then(e => {
    console.log(e);
    return e.text();
})
    .then(
        e => {
            document.body.innerHTML += e;
            console.log(text)
            console.log(e);
        });


function upoad_file(event) {
    event.preventDefault();

    var request = new Request('http://localhost:5114/upload', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-binary',
            'filename': event.target.myfile.files[0].name,
        },
        body: event.target.myfile.files[0]
    });

    console.log(request);

    /* fetch(request).then(response => {
         let status = response.status
         console.log("Response:");
         console.log(status);
         if (status === 202) {
             setFilesChanged(filesChanged + 1);
             event.target.myfile.value = null;
             //alert("File Upload completed");
         }
         else {
             response.text().then(message => alert(message));
         }
     });*/
}
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
    }).then(message =>
    {
        console.log(message);
        document.body.innerHTML += message;
    });

}
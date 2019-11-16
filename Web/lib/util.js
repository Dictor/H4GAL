function b64toBlob(b64Data, contentType, sliceSize) {
    contentType = contentType || '';
    sliceSize = sliceSize || 512;

    var byteCharacters = atob(b64Data);
    var byteArrays = [];

    for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
        var slice = byteCharacters.slice(offset, offset + sliceSize);

        var byteNumbers = new Array(slice.length);
        for (var i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }

        var byteArray = new Uint8Array(byteNumbers);

        byteArrays.push(byteArray);
    }

  var blob = new Blob(byteArrays, {type: contentType});
  return blob;
}
function RequestXhrGet(host, verb, okcb, errorcb) {
    var req = new XMLHttpRequest();
    req.open("GET", host + "/" + verb, true);
    req.onload = function() {
        if (req.status == 200) {
            okcb(req.response);
        } else {
            errorcb();
        }
    };
    req.send();
}
function RequestXhrPost(host, verb, param, okcb, errorcb) {
    var req = new XMLHttpRequest();
    req.open("POST", host + "/" + verb, true);
    req.setRequestHeader('Content-Type', 'application/json');
    req.onload = function() {
        if (req.status == 200) {
            okcb(req.response);
        } else {
            errorcb();
        }
    };
    req.send(param);
}
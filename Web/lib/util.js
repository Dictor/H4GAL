var global_host = window.location.protocol + "//" + window.location.hostname;

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

function RequestXhrGet(verb, okcb, errorcb) {
    var req = new XMLHttpRequest();
    req.open("GET", global_host + "/" + verb, true);
    req.onload = function() {
        if (req.status == 200) {
            okcb(req.response);
        } else {
            errorcb();
        }
    };
    req.send();
}

function RequestXhrPost(verb, param, okcb, errorcb) {
    var req = new XMLHttpRequest();
    req.open("POST", global_host + "/" + verb, true);
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
function ConvertErrorMessage(msg) {
    switch(msg) {
        case "INCORRECT_CODE": return "올바르지 않은 자격증명 코드";
        case "IMPROPER_CREDENTIAL": return "올바르지 않은 자격증명 정보";
        case "INVALID_SESSION": return "올바르지 않은 세션 정보";
        case "ALREADY_REGISTERED": return "이미 등록된 계정";
        default: return msg;
    }
}
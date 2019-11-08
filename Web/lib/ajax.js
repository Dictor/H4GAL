function GETApiReq(host, verb, okcb, errorcb) {
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

function POSTApiReq(host, verb, param, okcb, errorcb) {
    var req = new XMLHttpRequest();
    req.open("POST", host + "/" + verb, true);
    //req.setRequestHeader('Content-Type', 'application/json');
    req.onload = function() {
        if (req.status == 200) {
            okcb(req.response);
        } else {
            errorcb();
        }
    };
    req.send(param);
}
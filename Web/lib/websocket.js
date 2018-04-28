var ws = {
    socket : null,
    init : function() {
                    try{
                        var wsUrl = 'ws://localhost:81';
                        this.socket = new WebSocket(wsUrl);
                    }catch(e){
                        this.onError(null);
                    }
                    this.socket.onopen = function (evt) {ws.onOpen(evt)};
                    this.socket.onclose = function (evt) {ws.onClose(evt)};
                    this.socket.onmessage = function (evt) {ws.onMessage(evt)};
                    this.socket.onerror = function (evt) {ws.onError(evt)};
                },
    onOpen : function(evt) {
                    selector.onOpen(evt);
                },
    onMessage : function(evt) {
                    var rawdata = evt.data;
                    var pdata = rawdata.split("#");
                    var reqName = pdata[0];
                    try {
                        var reqData = JSON.parse(pdata[1]);
                    } catch (ex){
                        console.log("json parse error!")
                    }
                    selector.onMessage(reqName, reqData);
                },
    onError : function(evt) {
                    selector.onError(evt);       
                },
    onClose : function(evt) { 
                    selector.onClose(evt);
                },
    makeREQ : function(name, data){
                    this.socket.send(name + "#" + JSON.stringify(data));
                }
}
var selector = {
    nowPage : null,
    onOpen : function(evt) {
                    if (this.nowPage == "index"){
                        ctr_index.onOpen(evt);
                    } else if (this.nowPage == "disposableauth") {
                        ctr_disposableauth.onOpen(evt);
                    }
                },
    onMessage : function(name, data) {
                    if (this.nowPage == "index"){
                        ctr_index.onMessage(name, data);
                    } else if (this.nowPage == "disposableauth") {
                        ctr_disposableauth.onMessage(name, data);
                    }
                },
    onError : function(evt) {
                    if (this.nowPage == "index"){
                        ctr_index.onError(evt);
                    } else if (this.nowPage == "disposableauth") {
                        ctr_disposableauth.onError(evt);
                    }
                },
    onClose : function(evt) { 
                    if (this.nowPage == "index"){
                        ctr_index.onClose(evt);
                    } else if (this.nowPage == "disposableauth") {
                        ctr_disposableauth.onClose(evt);
                    }
                }
}
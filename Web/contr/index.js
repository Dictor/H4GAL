var ctr_index = {
    init : function(){
        window.onload = function () {
            $("#statusdiv_loginmenu").hide();
            $("#statusdiv_error").hide();			
            $("#statusdiv_conn").show();
            ws.init();
        }
    },
    login : function () {
        $("#login_btn").hide();
        $("#login_input").show();
    },
    onOpen : function(evt) {
            sid = getCookie("sid");
            if(!sid){
                ws.makeREQ("MAKESESSION", null);
            } else {
                ws.makeREQ("SESSION", {"sid": sid});
            }
    },
    onMessage : function(reqName, reqData) {
            if (reqName == "GETCREDENTIAL"){
                if (reqData["isNew"] == true){
                    $("#statusdiv_loginmenu").show();
                    $("#statusdiv_error").hide();			
                    $("#statusdiv_conn").hide();
                    ws.makeREQ("MAKESESSION", null);
                } else {
                    if (reqData["status"] == "empty"){
                        $("#statusdiv_loginmenu").show();
                        $("#statusdiv_error").hide();			
                        $("#statusdiv_conn").hide();
                    } else if  (reqData["status"] == "account"){
                    } else if  (reqData["status"] == "disposable"){
                    }
                }
            } else if (reqName == "ISSUESESSION"){
                sid = reqData["sid"];
                setCookie("sid", sid, 1);
                ws.makeREQ("SESSION", {"sid": sid})
            }
        },
    onError : function(evt) {
            $("#statusdiv_loginmenu").hide();
            $("#statusdiv_error").show();			
            $("#statusdiv_conn").hide();
        },
    onClose : function(evt) { 
            $("#statusdiv_loginmenu").hide();
            $("#statusdiv_error").show();			
            $("#statusdiv_conn").hide();
        }
}
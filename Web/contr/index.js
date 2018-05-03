var ctr_index = {
    init : function(){
        window.onload = function () {
            $("#statusdiv_loginmenu").hide();
            $("#statusdiv_error").hide();			
            $("#statusdiv_conn").show();
            ws.init();
        }
    },
    showlogininput : function () {
        $("#login_btn").hide();
        $("#login_input").show();
    },
    showdispinput : function () {
        $("#login_btn").hide();
        $("#disp_input").show();
    },
    reqdispauth : function () {
        var code = $("#txtAuthCode").val();
        if($("#txtStuID").val() && $("#txtName").val() && code){
            ws.makeREQ("TRYDISPAUTH", {"sid": sid, "name": encodeURIComponent($("#txtName").val()), "stuid": $("#txtStuID").val(), "code": SHA256(code + code.length)});
        }
    },
    reqaccauth : function () {
        var pw = $("#txtPW").val();
        if($("#txtID").val() && pw){
            ws.makeREQ("TRYACCAUTH", {"sid": sid, "id": $("#txtID").val(), "pw": SHA256(pw + pw.length)});
        }
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
            } else if (reqName == "TRYDISPAUTH"){
                if (reqData["status"]){
                    alert("자격증명을 취득했습니다.")
                    window.location.reload();
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                }
            } else if (reqName == "SHOWALERT"){
                alert(reqData["msg"]);
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
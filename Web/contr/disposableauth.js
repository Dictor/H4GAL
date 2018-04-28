var ctr_disposableauth = {
    init : function(){
        window.onload = function () {
            $("#txtQuestion").text("");
            $("#statusdiv_connecting").show();
            $("#statusdiv_success").hide();
            $("#statusdiv_error").hide();
            ws.init();
        }
    },
    tryAnswer : function() {
        if($("#txtAnswer").val()){
            ws.makeREQ("TRYANSWER", {"answer": encodeURIComponent($("#txtAnswer").val()), "sid": sid});
        }
    },
    onOpen : function(evt) {
        sid = getCookie("sid");
        if(!sid){
            alert("유효하지 않은 세션입니다! 초기페이지로 돌아갑니다.");
            location.href = "index.html";
        } else {
            ws.makeREQ("SESSION", {"sid": sid});
        }
    },
    onMessage : function(reqName, reqData) {
        if (reqName == "GETCREDENTIAL"){
            if (reqData["isNew"] == true){
                alert("유효하지 않은 세션입니다! 초기페이지로 돌아갑니다.");
                location.href = "index.html";
            } else {
                if (reqData["status"] == "empty"){
                    $("#statusdiv_success").show();
                    $("#statusdiv_error").hide();			
                    $("#statusdiv_connecting").hide();
                    ws.makeREQ("GETDISPQUESTION", {"sid": sid});
                } else if  (reqData["status"] == "account"){
                    alert("이미 계정 자격증명을 가지고 있습니다! 초기페이지로 돌아갑니다.");
                    location.href = "index.html";
                } else if  (reqData["status"] == "disposable"){
                    alert("이미 일회용 자격증명을 가지고 있습니다! 초기페이지로 돌아갑니다.");
                    location.href = "index.html";
                }
            }
        } else if (reqName == "GETDISPQUESTION") {
            $("#txtQuestion").text(decodeURIComponent(reqData["question"]));
        } else if (reqName == "TRYANSWER") {
            if(reqData["status"]){
                alert("자격증명 취득에 성공했습니다!");
                
            }else{
                alert("자격증명 취득에 실패했습니다, 다시시도해주세요.");
                $("#txtAnswer").val("");
            }
        }
    },
    onError : function(evt) {
        $("#statusdiv_connecting").hide();
        $("#statusdiv_success").hide();
        $("#statusdiv_error").show();
    },
    onClose : function(evt) { 
        $("#statusdiv_connecting").hide();
        $("#statusdiv_success").hide();
        $("#statusdiv_error").show();
    }
}
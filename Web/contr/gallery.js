var ctr_gallery = {
    init : function(){
            ws.init();
            window.onload = function() {$("#errbox").alert();};
        },
    ShowErr : function(detailtxt) {
            $("#errtxt").text(detailtxt);
            $("#errbox").show();
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
                        alert("권한이 없습니다! 초기페이지로 돌아갑니다.");
                        location.href = "index.html";
                    } else if  (reqData["status"] == "account"){
                        $("#txtAuthInfo").text = "사용자 : [일회용]" + reqData["name"]
                    } else if  (reqData["status"] == "disposable"){
                        $("#txtAuthInfo").text = "사용자 : " + reqData["name"]
                    }
                }
            }
        },
    onError : function(evt) {
            ctr_gallery.ShowErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요.")
        },
    onClose : function(evt) { 
        }
}
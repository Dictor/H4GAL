var ctr_index = {
    Init : function(){
        window.onload = function () {
            $("#status-login").show();
            $("#status-error").hide();			
            Kakao.init('47994d851f67e522d8665d374c804817');
            ctr_index.checkSession();
        }
    },
    ShowDispAuthMenu : function () {
        $("#login-menu").hide();
        $("#login-disp").show();
    },
    RequestKakaoAuth : function () {
        Kakao.Auth.login({
          success: function(authObj) {
            ws.makeREQ("TRYKAKAOAUTH", {"sid": sid, "token": Kakao.Auth.getAccessToken()});
          },
          fail: function(err) {
            alert("로그인 시도 중 오류가 발생했습니다! : " + JSON.stringify(err));
          }
        });
    },
    RequestDispAuth : function () {
        var code = $("#txtAuthCode").val();
        if($("#txtName").val() && code){  
            RequestXhrPost( 
                "http://" + window.location.hostname, 
                "auth/disp/try", 
                JSON.stringify({"name": encodeURIComponent($("#txtName").val()), "code": code}), 
                function(req) {console.log(req); ctr_index.ProcessResponce("TRYDISPAUTH", JSON.parse(req))}, 
                function() {alert("알 수 없는 API 오류!")}
            );
        }
    },
    checkSession : function(evt) {
            RequestXhrGet(  
                window.location.protocol + "//" + window.location.hostname, 
                "session/check",
                function(req){
                    var preq = JSON.parse(req);
                    console.log("Session : ", preq)
                    if (preq["is_new"] === true) {
                        RequestXhrGet(  
                            "http://" + window.location.hostname, 
                            "session/assign",
                            null,
                            function() {alert("알 수 없는 API 오류!")}
                        );
                    } else {
                        if (preq["status"] != "empty") location.href = "gallery.html";
                    }
                }, 
                function() {alert("알 수 없는 API 오류!")}
            );
    },
    ProcessResponce : function(reqName, reqData) {
            if (reqName == "TRYDISPAUTH"){
                if (reqData["status"] === true){
                    alert("자격증명을 취득했습니다.")
                    window.location.reload();
                } else {
                    alert("오류가 발생했습니다! (" + ConvertErrorMessage(reqData["error"]) + ")");
                }
            } else if (reqName == "TRYKAKAOAUTH"){
                if (reqData["status"]){
                    window.location.reload();
                } else {
                    if (reqData["error"] == "NEED_REGISTER") {
                        if(confirm("카카오 로그인을 이용하기 위한 등록 과정이 필요합니다. 등록을 진행하시겠습니까?")){
                            var code = prompt("등록을 위해 관리자로 부터 제공받은 자격증명코드를 입력해주세요.");
                            ws.makeREQ("REGISTERKAKAOAUTH",{"sid": sid, "token": Kakao.Auth.getAccessToken(), "code": code});
                        }
                    } else {
                        alert("오류가 발생했습니다! (" + ConvertErrorMessage(reqData["error"]) + ")");
                    }
                }
            } else if (reqName == "REGISTERKAKAOAUTH"){
                if (reqData["status"]){
                    alert("등록에 성공했습니다. 다시 카카오 로그인을 진행해주세요.")
                } else {
                    alert("오류가 발생했습니다! (" + ConvertErrorMessage(reqData["error"]) + ")");
                }
            } else if (reqName == "SHOWALERT"){
                alert(reqData["msg"]);
            }
        },
    onError : function(evt) {
        $("#statusdiv-login").hide();
        $("#statusdiv-error").show();			
    }
}
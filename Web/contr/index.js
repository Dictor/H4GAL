var ctr_index = {
    init : function(){
        window.onload = function () {
            $("#statusdiv_loginmenu").hide();
            $("#statusdiv_error").hide();			
            $("#statusdiv_conn").show();
            Kakao.init('47994d851f67e522d8665d374c804817');
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
    loginWithKakao : function () {
        Kakao.Auth.login({
          success: function(authObj) {
            ws.makeREQ("TRYKAKAOAUTH", {"sid": sid, "token": Kakao.Auth.getAccessToken()});
          },
          fail: function(err) {
            alert("로그인 시도 중 오류가 발생했습니다! : " + JSON.stringify(err));
          }
        });
    },
    reqdispauth : function () {
        var code = $("#txtAuthCode").val();
        if($("#selclass").val() && $("#txtName").val() && code){     
            ws.makeREQ("TRYDISPAUTH", {"sid": sid, "name": encodeURIComponent($("#txtName").val()), "stuid": $("#selclass").val(), "code": SHA256(code + code.length)});
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
                        location.href = "gallery.html";
                    } else if  (reqData["status"] == "disposable"){
                        location.href = "gallery.html";
                    } else if  (reqData["status"] == "kakao"){
                        location.href = "gallery.html";
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
            } else if (reqName == "TRYKAKAOAUTH"){
                if (reqData["status"]){
                    window.location.reload();
                } else {
                    switch(reqData["error"]) {
                        case "NEED_REGISTER":
                            if(confirm("카카오 로그인을 이용하기 위한 등록 과정이 필요합니다. 등록을 진행하시겠습니까?")){
                                var code = prompt("등록을 위해 관리자로 부터 제공받은 자격증명코드를 입력해주세요.");
                                ws.makeREQ("REGISTERKAKAOAUTH",{"sid": sid, "token": Kakao.Auth.getAccessToken(), "code": SHA256(code + code.length)});
                            }
                            break;
                        case "ILLEGAL_CREDENTIAL":
                            alert("오류가 발생했습니다! (올바르지 않은 카카오 자격증명 정보)");
                            break;
                        case "INVALID_SESSION":
                            alert("오류가 발생했습니다! (올바르지 않은 세션)");
                            break;
                        default:
                            alert("오류가 발생했습니다!");
                            break;
                    }
                }
            } else if (reqName == "REGISTERKAKAOAUTH"){
                if (reqData["status"]){
                    alert("등록에 성공했습니다. 다시 카카오 로그인을 진행해주세요.")
                } else {
                    switch(reqData["error"]) {
                        case "INCORRECT_CODE":
                            alert("오류가 발생했습니다! (올바르지 않은 자격증명코드)");
                            break;
                        case "ILLEGAL_CREDENTIAL":
                            alert("오류가 발생했습니다! (올바르지 않은 카카오 자격증명 정보)");
                            break;
                        case "INVALID_SESSION":
                            alert("오류가 발생했습니다! (올바르지 않은 세션)");
                            break;
                        case "ALREADY_REGISTERED":
                            alert("오류가 발생했습니다! (이미 등록된 계정)");
                            break;
                        default:
                            alert("오류가 발생했습니다!");
                            break;
                    }
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
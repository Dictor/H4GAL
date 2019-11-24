var ctr_index = {
    requestDispAuth : function () {
        var ucode = $("#index-login-disp-code").val(), uname = $("#index-login-disp-name").val();
        if(uname && ucode){  
            RequestXhrPost( 
                "auth/disp/try", 
                JSON.stringify({"name": encodeURIComponent(uname), "code": ucode}), 
                function(req) {
                    var preq = JSON.parse(req);
                    if (preq["status"]){
                        window.location.reload();
                    } else {
                        alert("오류가 발생했습니다! (" + ConvertErrorMessage(preq["error"]) + ")");
                    }
                }, 
                function() {alert("알 수 없는 API 오류!")}
            );
        }
    },
    checkSession : function(evt) {
            RequestXhrGet(  
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
    showDispMenu: function() {
        $("#index-login-select").hide();
        $("#index-login-disp").show();
    }
}
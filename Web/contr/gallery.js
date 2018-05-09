var ctr_gallery = {
    nowdir : "/",
    init : function(){
            ws.init();
            window.onload = function() {$("#errbox").alert();};
        },
    ShowErr : function(detailtxt) {
            $("#errtxt").text(detailtxt);
            $("#errbox").show();
        },
    ShowImgList : function() {
            ws.makeREQ("GETIMGLIST", {"sid": sid, "dir": ctr_gallery.nowdir});
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
                    } else if  (reqData["status"] == "disposable"){
                        $("#txtAuthInfo").text("사용자 : [일회용]" + reqData["name"]);
                        ctr_gallery.ShowImgList();
                    } else if  (reqData["status"] == "account"){
                        $("#txtAuthInfo").text("사용자 : " + reqData["name"]);
                        ctr_gallery.ShowImgList();
                    }
                }
            } else if (reqName == "GETIMGLIST"){
                if (reqData["status"]){
                    console.log(reqData);
                    var nowrow = 0, nowcol = 0;
                    var htmlstring = '<div class="row imgdiv">';
                    for(var i = 0; i < reqData["result"].length; i++){
                        var nowele = reqData["result"][i];
                        if(nowele["type"] == "ALBUM"){
                            htmlstring += '<div class="col-md-4"><a href="#" class="thumbnail"><img src="./img/hamster.png"><div class="caption"><p class="thtitle">' + nowele["title"] + '</p><p class="thdetail">' + nowele["detail"] + '</p></div></a></div>';
                        } else {
                            htmlstring += '<div class="col-md-4"><a href="#" class="thumbnail"><img src="./img/hamster.png"></a></div>';
                        }
                        if(i % 3 == 2 && i != 0){
                            htmlstring += '</div><div class="row imgdiv">';
                        }
                    }
                    htmlstring += "</div>";
                    $("#imgdata").html(htmlstring);
                    ws.makeREQ("GETIMG", {"sid": sid, "dir": "hamster.jpg"})
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                }
            } else if (reqName == "GETIMG"){
                if (reqData["status"]){
                    console.log(reqData);
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                }
            }
        },
    onError : function(evt) {
            ctr_gallery.ShowErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요.");
        },
    onClose : function(evt) {
            ctr_gallery.ShowErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요."); 
        }
}
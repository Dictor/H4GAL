var ctr_gallery = {
    nowdir : "/",
    imgbase64data : {},
    imgbindlist : {},
    init : function(){
            ws.init();
            window.onload = function() {$("#errbox").alert();};
        },
    showErr : function(detailtxt) {
            $("#errtxt").text(detailtxt);
            $("#errbox").show();
        },
    showImgList : function() {
            ws.makeREQ("GETIMGLIST", {"sid": sid, "dir": ctr_gallery.nowdir});
        },
    bindThumbnail : function(){
            var delfunc = function(gap) { /* gap is in millisecs */
                var then,now;
                then = new Date().getTime();
                now = then;
                while((now - then) < gap) {
                    now = new Date().getTime();
                }
            };
            for(var key in ctr_gallery.imgbindlist){
                ws.makeREQ("GETIMG", {"sid": sid, "dir": key});
                delfunc(100);
                console.log(key);
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
                        alert("권한이 없습니다! 초기페이지로 돌아갑니다.");
                        location.href = "index.html";
                    } else if  (reqData["status"] == "disposable"){
                        $("#txtAuthInfo").text("사용자 : [일회용]" + reqData["name"]);
                        ctr_gallery.showImgList();
                    } else if  (reqData["status"] == "account"){
                        $("#txtAuthInfo").text("사용자 : " + reqData["name"]);
                        ctr_gallery.showImgList();
                    }
                }
            } else if (reqName == "GETIMGLIST"){
                if (reqData["status"]){
                    var nowrow = 0, nowcol = 0;
                    var htmlstring = '<div class="row imgdiv">';
                    for(var i = 0; i < reqData["result"].length; i++){
                        var nowele = reqData["result"][i];
                        if(nowele["type"] == "ALBUM"){
                            htmlstring += '<div class="col-md-4"><a href="#" class="thumbnail"><img id="thimg' + i + '" src="./img/hamster.png"><div class="caption"><p class="thtitle">' + nowele["title"] + '</p><p class="thdetail">' + nowele["detail"] + '</p></div></a></div>';
                        } else {
                            htmlstring += '<div class="col-md-4"><a href="#" class="thumbnail"><img id="thimg' + i + '" src="./img/hamster.png"></a></div>';
                        }
                        if(nowele["thimg"] != "NONE"){ctr_gallery.imgbindlist[nowele["thimg"]] = "thimg" + i;}
                        if(i % 3 == 2 && i != 0){
                            htmlstring += '</div><div class="row imgdiv">';
                        }
                    }
                    htmlstring += "</div>";
                    $("#imgdata").html(htmlstring);
                    ctr_gallery.bindThumbnail();
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                }
            } else if (reqName == "GETIMG"){
                if (reqData["status"]){
                    if (reqData["task"] == "start"){
                        ctr_gallery.imgbase64data[reqData["name"]] = new Array();
                    } else if (reqData["task"] == "slice"){
                        ctr_gallery.imgbase64data[reqData["name"]][reqData["slicecount"]] = reqData["data"];
                    } else if (reqData["task"] == "end"){
                        var compbase64 = "";
                        for(var i = 0; i < (ctr_gallery.imgbase64data[reqData["name"]]).length; i++){
                            compbase64 += ctr_gallery.imgbase64data[reqData["name"]][i];
                        }
                        $("#" + ctr_gallery.imgbindlist[reqData["name"]]).attr("src",window.URL.createObjectURL(b64toBlob(compbase64, "data:image")));
                    }
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                }
            }
        },
    onError : function(evt) {
            ctr_gallery.showErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요.");
        },
    onClose : function(evt) {
            ctr_gallery.showErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요."); 
        }
}
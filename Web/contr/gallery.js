var ctr_gallery = {
    nowdir : "/",
    imgbase64data : {},
    imgbindlist : {},
    autothbindlist : {},
    init : function(){
            ws.init();
            window.onload = function() {
                $("#errbox").alert();
                $(".loadPopup").hide();
                $(".viewPopup").hide();
            };
        },
    showErr : function(detailtxt) {
            $("#errtxt").text(detailtxt);
            $("#errbox").show();
        },
    showImgList : function() {
            ws.makeREQ("GETIMGLIST", {"sid": sid, "dir": encodeURI(ctr_gallery.nowdir)});
        },
    go : function(isalbum, dir) {
        if (isalbum) {
            ctr_gallery.nowdir += dir;
            $(".loadPopup").show();
            ctr_gallery.showImgList();
        } else {
            $(".loadPopup").show();
            console.log(ctr_gallery.nowdir + dir);
            ctr_gallery.imgbindlist[ctr_gallery.nowdir + dir] = "vpimg";
            ws.makeREQ("GETIMG", {"sid": sid, "dir": encodeURI(ctr_gallery.nowdir + dir)});
        }
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
                delfunc(50);
            }
            for(var key in ctr_gallery.autothbindlist){
                ws.makeREQ("GETTHUMB", {"sid": sid, "thid": key});
                delfunc(50);
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
                    var htmlstring = '<div class="row imgdiv">';
                    $(".loadPopup").show();
                    for(var i = 0; i < reqData["result"].length; i++){
                        var nowele = reqData["result"][i];
                        if(nowele["type"] == "ALBUM"){
                            htmlstring += '<div class="col-md-4" onclick="ctr_gallery.go(true,' + "'" + nowele["dir"] + "'" + ');"><a class="thumbnail"><img id="thimg' + i + '" src="./img/hamster.png"><div class="caption"><p class="thtitle">' + nowele["title"] + '</p><p class="thdetail">' + nowele["detail"] + '</p></div></a></div>';
                        } else {
                            htmlstring += '<div class="col-md-4" onclick="ctr_gallery.go(false,' + "'" + nowele["dir"] + "'" + ');"><a class="thumbnail"><img id="thimg' + i + '" src="./img/hamster.png"><div class="caption"><p class="thtitle">' + nowele["title"] + '</p></div></a></div>';
                        }
                        if(nowele["thimg"] != "NONE"){
                            if(nowele["isautoth"] == true){
                                ctr_gallery.autothbindlist[nowele["thimg"]] = "thimg" + i;
                            } else {
                                ctr_gallery.imgbindlist[nowele["thimg"]] = "thimg" + i;
                            }
                        }
                        if(i % 3 == 2 && i != 0){
                            htmlstring += '</div><div class="row imgdiv">';
                        }
                    }
                    htmlstring += "</div>";
                    $("#imgdata").html(htmlstring);
                    ctr_gallery.bindThumbnail();
                    $(".loadPopup").hide();
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                    ctr_gallery.nowdir = "/";
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
                        if (ctr_gallery.imgbindlist[reqData["name"]] == "vpimg"){
                            $(".loadPopup").hide();
                            $("#vptitle").text(reqData["name"].split("/").slice(-1).pop());
                            $(".viewPopup").show();
                            ctr_gallery.resizevpimg();
                            $(window).resize(function(){
                                ctr_gallery.resizevpimg();
                            });
                        }
                    }
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                }
            } else if (reqName == "GETTHUMB"){
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
                        $("#" + ctr_gallery.autothbindlist[reqData["name"]]).attr("src",window.URL.createObjectURL(b64toBlob(compbase64, "data:image")));
                    }
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                }
            }
        },
    resizevpimg : function() {
        if($("#vpimgcover").width() >= $("#vpimgcover").height()){
            var calch = $("#vpimgcover").height() - ( $("#vptitle").outerHeight(true)+ $("#vpdesc").outerHeight(true));
            $("#vpimg").outerHeight(calch);
            $("#vpimg").width("auto");
            if($("#vpimg").width() > $("#vpimgcover").width()){
                $("#vpimg").outerWidth($("#vpimgcover").width());
                $("#vpimg").height("auto");
            }
        } else {
            $("#vpimg").outerWidth($("#vpimgcover").width());
            $("#vpimg").height("auto");
        }
    },
    onError : function(evt) {
            ctr_gallery.showErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요.");
        },
    onClose : function(evt) {
            ctr_gallery.showErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요."); 
        }
}
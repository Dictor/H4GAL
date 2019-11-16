var ctr_gallery = {
    nowdir : "/",
    gimgCount : 0,
    photoperpage : 15,
    nowimglst : {},
    imgbase64data : {},
    imgbindlist : {},
    autothbindlist : {},
    init : function(){
        window.onload = function() {
            $("#gallery-error-box").alert();
            $(".gallery-load-popup").hide();
            $(".gallery-view-popup").hide();
        };
    },
    ShowErrorPopup : function(detail) {
        $("#gallery-error-box-txt").text(detailtxt);
        $("gallery-error-box").show();
    },
    showImgList : function() {
            ws.makeREQ("GETIMGLIST", {"sid": sid, "dir": encodeURI(ctr_gallery.nowdir)});
        },
    go : function(isalbum, dir) {
        if (isalbum) {
            ctr_gallery.nowdir += dir;
            $(".gallery-load-popup").show();
            ctr_gallery.showImgList();
        } else {
            $(".gallery-load-popup").show();
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
                ws.makeREQ("GETIMG", {"sid": sid, "dir":  encodeURI(key)});
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
                        $(".gtitletxt").css("display") == "none" ? $("#txtAuthInfo").text("D" + reqData["name"]) : $("#txtAuthInfo").text("사용자 : D" + reqData["name"]);
                        ctr_gallery.showImgList();
                    } else if  (reqData["status"] == "kakao"){
                        $(".gtitletxt").css("display") == "none" ? $("#txtAuthInfo").text("K" + reqData["name"]) : $("#txtAuthInfo").text("사용자 : K" + reqData["name"]);
                        ctr_gallery.showImgList();
                    } else if  (reqData["status"] == "account"){
                        $(".gtitletxt").css("display") == "none" ? $("#txtAuthInfo").text(reqData["name"]) : $("#txtAuthInfo").text("사용자 : " + reqData["name"]);
                        ctr_gallery.showImgList();
                    }
                }
            } else if (reqName == "GETIMGLIST"){
                if (reqData["status"]){
                    $('.page').bootpag({
                        total: Math.ceil(reqData["result"].length / ctr_gallery.photoperpage),
                        page: 1,
                        maxVisible: 10
                     }).on('page', function(event, num){
                        ctr_gallery.showpage(num)
                     });
                    ctr_gallery.nowimglst = $.extend(true, {}, reqData["result"]);
                    ctr_gallery.showpage(1)
                } else {
                    alert("오류가 발생했습니다! (" + reqData["error"] + ")");
                    ctr_gallery.nowdir = "/";
                }
            } else if (reqName == "GETIMG"){
                if (reqData["status"]){
                    if (reqData["task"] == "start"){
                        ctr_gallery.imgbase64data[reqData["name"]] = new Array();
                        ctr_gallery.gimgCount = Number(reqData["count"]);
                    } else if (reqData["task"] == "slice"){
                        ctr_gallery.imgbase64data[reqData["name"]][reqData["slicecount"]] = reqData["data"];
                        $("#loadpopupPerc").text(String(Math.floor((Number(reqData["slicecount"])/ctr_gallery.gimgCount)*100)) + "%");
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
                            $("#vpfiledw").attr("href", $('#vpimg').attr('src'));
                            $("#vpfiledw").attr("download", $("#vptitle").text());
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
    showpage : function(pagenum) {
        $(".loadPopup").show();
        var htmlstring = '<div class="row imgdiv">';
        ctr_gallery.autothbindlist = {};
        ctr_gallery.imgbindlist = {};
        for(var i = ctr_gallery.photoperpage * (pagenum - 1); i < (ctr_gallery.photoperpage * (pagenum - 1)) + ctr_gallery.photoperpage; i++){
            if (!ctr_gallery.nowimglst[i]) break;
            var nowele = ctr_gallery.nowimglst[i];
            if(nowele["type"] == "ALBUM"){
                htmlstring += '<div class="col-md-4" onclick="ctr_gallery.go(true,' + "'" + nowele["dir"] + "'" + ');"><a class="thumbnail"><img id="thimg' + i + '" src="./img/hamster.png"><div class="caption"><p class="thtitle">' + nowele["title"] + '</p><p class="thdetail">' + nowele["detail"] + '</p></div></a></div>';
            } else {
                htmlstring += '<div class="col-md-4" onclick="ctr_gallery.go(false,' + "'" + nowele["dir"] + "'" + ');"><a class="thumbnail"><img id="thimg' + i + '" src="./img/hamster.png"><div class="caption"><p class="thtitle">' + nowele["title"] + '</p></div></a></div>';
            }
            if(nowele["thimg"] != "NONE"){
                ctr_gallery.autothbindlist[nowele["thimg"]] = "thimg" + i;
            }
            if(i % 3 == 2 && i != 0){
                htmlstring += '</div><div class="row imgdiv">';
            }
        }
        htmlstring += "</div>";
        $("#imgdata").html(htmlstring);
        ctr_gallery.bindThumbnail();
        $(".loadPopup").hide();
    },
    resizevpimg : function() {
        var picdispW = $("#vpimgcover").width();
        var picdispH = $("#vprow").height();
        if(picdispW >= picdispH){
            var calch = 0;
            if($("#vpimgcover").css("float") == "left"){                
                calch = picdispH - ($("#vptitle").outerHeight(true));
            } else {
                calch = picdispH - ($("#vpimgbefore").outerHeight(true) + $("#vptitle").outerHeight(true) + $("#vpother").outerHeight(true));
            }
            $("#vpimg").outerHeight(calch);
            $("#vpimg").width("auto");
            if($("#vpimg").width() > picdispW){
                $("#vpimg").outerWidth(picdispW);
                $("#vpimg").height("auto");
            }
            $("#vpimgcover").height($("#vpimg").outerHeight(true) + $("#vptitle").outerHeight(true));
        } else {
            $("#vpimg").outerWidth(picdispW);
            $("#vpimg").height("auto");
            $("#vpimgcover").height($("#vpimg").outerHeight(true) + $("#vptitle").outerHeight(true));
        }
        
        if($("#vpimgcover").css("float") == "left"){
            $(".vpothercont").addClass("vpother_vert");
            $(".vpothercont").removeClass("vpother_horz");
        } else {
            $(".vpothercont").removeClass("vpother_vert");
            $(".vpothercont").addClass("vpother_horz");
        }
    },
    showImgPopup : function(url) {
        var imgWindow = window.open("", "하늘고 4기 사진창고");
		imgWindow.document.write("<img src='" + url + "'>");
    },
    onError : function(evt) {
            ctr_gallery.showErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요.");
        },
    onClose : function(evt) {
            ctr_gallery.showErr("서버와 연결중 오류가 발생했습니다! 페이지를 새로고쳐 재시도 하세요."); 
        }
}
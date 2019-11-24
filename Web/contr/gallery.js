var ctr_gallery = {
    nowdir : "/",
    photoperpage : 15,
    nowimglst : {},
    imgbindlist : {},
    autothbindlist : {},
    init : function(){
        window.onload = function() {
            $("#gallery-error-box").alert();
            $(".gallery-load-popup").hide();
            $(".gallery-view-popup").hide();
            ctr_gallery.checkSession();
        };
    },
    ShowErrorPopup : function(detail) {
        $("#gallery-error-box-txt").text(detailtxt);
        $("gallery-error-box").show();
    },
    showImageList : function() {
            RequestXhrGet(  
                window.location.protocol + "//" + window.location.hostname, 
                "list?dir=" + ctr_gallery.nowdir,
                function(req){
                    var preq = JSON.parse(req);
                    if (preq["status"]) {
                        $('.page').bootpag({
                            total: Math.ceil(preq["result"].length / ctr_gallery.photoperpage),
                            page: 1,
                            maxVisible: 10
                        }).on('page', function(event, num){
                            ctr_gallery.showPage(num)
                        });
                        ctr_gallery.nowimglst = $.extend(true, {}, preq["result"]);
                        ctr_gallery.showPage(1)
                    } else {
                        alert("알 수 없는 오류! (" + ConvertErrorMessage(preq["error"]) + ")");
                        ctr_gallery.nowdir = "/";
                    }
                }, 
                function() {alert("알 수 없는 API 오류!")}
            );
        },
    go : function(isalbum, dir) {
        if (isalbum) {
            ctr_gallery.nowdir += dir + "/";
            $(".gallery-load-popup").show();
            ctr_gallery.showImageList();
        } else {
            $(".gallery-load-popup").show();
            ctr_gallery.imgbindlist[ctr_gallery.nowdir + dir] = "vpimg";
            ctr_gallery.getImage(encodeURI(ctr_gallery.nowdir + dir));
        }
    },
    bindThumbnail : function(){
        for(var key in ctr_gallery.imgbindlist){
            ctr_gallery.getImage(encodeURI(key));
        }
        for(var key in ctr_gallery.autothbindlist){
            ctr_gallery.getThumbnail(key)
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
                        alert("유효하지 않은 세션입니다! 초기 페이지로 돌아갑니다.");
                        location.href = "index.html";
                    } else {
                        switch(preq["status"]) {
                            case "empty": 
                                alert("유효하지 않은 자격증명입니다! 초기 페이지로 돌아갑니다.");
                                location.href = "index.html";
                            case "disp":
                                $(".gtitletxt").css("display") == "none" ? $("#txtAuthInfo").text("D" + preq["name"]) : $("#txtAuthInfo").text("사용자 : D" + preq["name"]);
                                ctr_gallery.showImageList();
                                break;
                        }
                    }
                }, 
                function() {alert("알 수 없는 API 오류!")}
            );
    },    
    getImage : function (dir) {
        RequestXhrGet(  
            window.location.protocol + "//" + window.location.hostname, 
            "image?dir=" + dir,
            function(req){
                var preq = JSON.parse(req);
                if (preq["status"]) {
                    $("#popup-image").attr("src",window.URL.createObjectURL(b64toBlob(preq["image"], "data:image")));
                    if (ctr_gallery.imgbindlist[dir] == "vpimg"){
                        $(".gallery-load-popup").hide();
                        $("#image-title").text(dir.split("/").slice(-1).pop());
                        $(".gallery-view-popup").show();
                        $("#vpfiledw").attr("href", $('#vpimg').attr('src'));
                        $("#vpfiledw").attr("download", $("#image-title").text());
                        ctr_gallery.resizevpimg();
                        $(window).resize(function(){
                            ctr_gallery.resizevpimg();
                        });
                    }
                } else {
                    console.error("Fail to get image :", preq)
                }
            }, 
            function() {alert("알 수 없는 API 오류!")}
        );
    },
    getThumbnail : function (id) {
        RequestXhrGet(  
            window.location.protocol + "//" + window.location.hostname, 
            "thumb?id=" + id,
            function(req){
                var preq = JSON.parse(req);
                if (preq["status"]) {
                    $("#" + ctr_gallery.autothbindlist[id]).attr("src",window.URL.createObjectURL(b64toBlob(preq["image"], "data:image")));
                } else {
                    console.error("Fail to get thumbnail :", preq)
                }
            }, 
            function() {alert("알 수 없는 API 오류!")}
        );
    },
    showPage : function(pagenum) {
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
        var picdispW = $("#image-cover").width();
        var picdispH = $("#vprow").height();
        if(picdispW >= picdispH){
            var calch = 0;
            if($("#image-cover").css("float") == "left"){                
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
            $("#image-cover").height($("#vpimg").outerHeight(true) + $("#vptitle").outerHeight(true));
        } else {
            $("#vpimg").outerWidth(picdispW);
            $("#vpimg").height("auto");
            $("#image-cover").height($("#vpimg").outerHeight(true) + $("#vptitle").outerHeight(true));
        }
        
        if($("#image-cover").css("float") == "left"){
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
    }
}
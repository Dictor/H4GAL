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

var ctr_gallery = {
    currentPath : "/",
    currentImageResult: {},
    CONST_IMAGE_PER_PAGE: 15,
    showInit : function() {
        ctr_gallery.getPage(1); 
        ctr_gallery.showPage(1);
    },
    getPage : function(page_num) {
        var preq = JSON.parse(RequestXhrGetSync("list?dir=" + ctr_gallery.currentPath));
        if (preq["status"]) {
            ctr_gallery.currentImageResult = preq["result"];
        } else {
            alert("알 수 없는 오류! (" + ConvertErrorMessage(preq["error"]) + ")");
            ctr_gallery.currentPath = "/";
        }
    },
    showPage : function(page_num) { //1 부터 시작
        document.getElementById("gallery-image-result").innerHTML = "";
        for(var i = (page_num - 1) * ctr_gallery.CONST_IMAGE_PER_PAGE; i < page_num * ctr_gallery.CONST_IMAGE_PER_PAGE; i++) {
            if (i >= ctr_gallery.currentImageResult.length) break;
            var current_th = ctr_gallery.currentImageResult[i]["thimg"] == "NONE" ? "img/hamster.png" : ctr_gallery.makeThumbnailBlob(ctr_gallery.currentImageResult[i]["thimg"]);
            var is_album = ctr_gallery.currentImageResult[i]["type"] == "ALBUM" ? "true" : "false";
            document.getElementById("gallery-image-result").innerHTML +=
                '<div class="card" onclick="javascript:ctr_gallery.goTo(' + is_album + ",'" + ctr_gallery.currentImageResult[i]["dir"] + '\')"><img src="' + current_th + 
                '" class="card-img-top"><div class="card-body"><h5 class="card-title">' + ctr_gallery.currentImageResult[i]["title"] +
                '</h5><p class="card-text">' + ctr_gallery.currentImageResult[i]["detail"] + '</p></div>';
        }
    },
    showPagination: function(total_page, current_page) {
        for (var i = 1; i <= total_page; i++) {
            document.getElementsByClassName("gallery-pagination").innerHTML += 
                '<li class="page-item"><a class="page-link" href="javascript:ctr_gallery.showPage(' + i + ')">' + i + '</a></li>'
        }
    },
    goTo : function(is_album, dir) {
        if (is_album) {
            ctr_gallery.currentPath += dir;
            ctr_gallery.getPage(1);
            ctr_gallery.showPage(1);
        } else {
            
        }
    },
    makeImageBlob : function (dir) {
        var preq = JSON.parse(RequestXhrGetSync("image?dir=" + dir));
        if (preq["status"]) {
            return window.URL.createObjectURL(b64toBlob(preq["image"], "data:image"));
        } else {
            console.error("Fail to get image :", preq)
            return null;
        }
    },
    makeThumbnailBlob : function (id) {
        var preq = JSON.parse(RequestXhrGetSync("thumb?id=" + id));
        if (preq["status"]) {
            return window.URL.createObjectURL(b64toBlob(preq["image"], "data:image"));
        } else {
            console.error("Fail to get thumbnail :", preq);
            return null;
        }
    }
}
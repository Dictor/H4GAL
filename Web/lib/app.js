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
    currentThumbnail: {},
    CONST_IMAGE_PER_PAGE: 15,
    showInit : function() {
        ctr_gallery.getPageInfo(1); 
    },
    getPageInfo : function(page_num) {
        RequestXhrGet(  
            "list?dir=" + ctr_gallery.currentPath,
            function(req){
                var preq = JSON.parse(req);
                if (preq["status"]) {
                    ctr_gallery.currentImageResult = preq["result"];
                    ctr_gallery.getAllThumbnail(function() {ctr_gallery.showImage(page_num);}); 
                } else {
                    alert("알 수 없는 오류! (" + ConvertErrorMessage(preq["error"]) + ")");
                    ctr_gallery.currentPath = "/";
                }
            }, 
            function() {alert("알 수 없는 API 오류!")}
        );
    },
    showPagination: function(total_page, current_page) {
        for (var i = 1; i <= total_page; i++) {
            document.getElementsByClassName("gallery-pagination").innerHTML += 
                '<li class="page-item"><a class="page-link" href="javascript:ctr_gallery.showPage(' + i + ')">' + i + '</a></li>'
        }
    },
    getAllThumbnail : function(complete_callback) {
        for (var i = 0; i < ctr_gallery.currentImageResult.length; i++) {
           if (ctr_gallery.currentImageResult[i]["thimg"] != "NONE") {
                ctr_gallery.getThumbnail(
                    ctr_gallery.currentImageResult[i]["thimg"], 
                    function (id, data) {
                        ctr_gallery.currentThumbnail[id] = data;
                        if(i  == ctr_gallery.currentImageResult.length) complete_callback();
                    }
                );
            } else {
                now_thumb_src = "img/hamster.png";
            }   
        }
    },
    showImage : function(page_num) { //1 부터 시작
        for(var i = (page_num - 1) * ctr_gallery.CONST_IMAGE_PER_PAGE; i < page_num * ctr_gallery.CONST_IMAGE_PER_PAGE; i++) {
            if (i >= ctr_gallery.currentImageResult.length) break;
            var current_th = ctr_gallery.currentImageResult[i]["thimg"] == "NONE" ? "img/hamster.png" : ctr_gallery.currentThumbnail[ctr_gallery.currentImageResult[i]["thimg"]];
            document.getElementById("gallery-image-result").innerHTML +=
                '<div class="card"><img src="' + current_th + 
                '" class="card-img-top"><div class="card-body"><h5 class="card-title">' + ctr_gallery.currentImageResult[i]["title"] +
                '</h5><p class="card-text">' + ctr_gallery.currentImageResult[i]["detail"] + '</p></div>';
        }
    },
    getImage : function (dir) {
        RequestXhrGet(  
            "image?dir=" + dir,
            function(req){
                var preq = JSON.parse(req);
                if (preq["status"]) {
                    return window.URL.createObjectURL(b64toBlob(preq["image"], "data:image"));
                } else {
                    console.error("Fail to get image :", preq)
                }
            }, 
            function() {alert("알 수 없는 API 오류!")}
        );
    },
    getThumbnail : function (id, return_callback) {
        RequestXhrGet(  
            "thumb?id=" + id,
            function(req){
                var preq = JSON.parse(req);
                if (preq["status"]) {
                    return_callback(id, window.URL.createObjectURL(b64toBlob(preq["image"], "data:image")));
                } else {
                    console.error("Fail to get thumbnail :", preq)
                }
            }, 
            function() {alert("알 수 없는 API 오류!")}
        );
    }
}
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
    currentImageName : "",
    currentImageResult: {},
    CONST_IMAGE_PER_PAGE: 12,
    processPage : async function(page_num) {
        await ctr_gallery.checkSession();
        await ctr_gallery.getPage(page_num); 
        await ctr_gallery.showPage(page_num);
        ctr_gallery.showPagination(Math.ceil(ctr_gallery.currentImageResult.length / ctr_gallery.CONST_IMAGE_PER_PAGE), page_num);
    },
    processView : async function(dir) {
        document.getElementById("gallery-image").src = "";
        document.getElementById("gallery-popup").classList.remove("gallery-hide");
        document.getElementById("gallery-image").src = await ctr_gallery.makeImageBlob(dir);
    },
    checkSession : async function() {
        var preq = JSON.parse(await RequestXhrGetPromise("session/check"));
        if (preq["is_new"] ||  preq["status"] == "empty") {
            alert("권한이 없습니다! 메인 페이지로 돌아갑니다.");
        } else {
            document.getElementById("gallery-navbar-username").innerHTML = preq["name"];
        }
    },
    getPage : async function(page_num) {
        var preq = JSON.parse(await RequestXhrGetPromise("list?dir=" + ctr_gallery.currentPath));
        if (preq["status"]) {
            ctr_gallery.currentImageResult = preq["result"];
        } else {
            alert("알 수 없는 오류! (" + ConvertErrorMessage(preq["error"]) + ")");
            ctr_gallery.currentPath = "/";
        }
    },
    showPage : async function(page_num) { //1 부터 시작
        document.getElementById("gallery-image-result").innerHTML = "";
        for(var i = (page_num - 1) * ctr_gallery.CONST_IMAGE_PER_PAGE; i < page_num * ctr_gallery.CONST_IMAGE_PER_PAGE; i++) {
            if (i >= ctr_gallery.currentImageResult.length) break;
            var current_th = ctr_gallery.currentImageResult[i]["thimg"] == "NONE" ? "img/hamster.png" : (await ctr_gallery.makeThumbnailBlob(ctr_gallery.currentImageResult[i]["thimg"]));
            current_th = (current_th === null ? "img/hamster.png" : current_th);
            var is_album = ctr_gallery.currentImageResult[i]["type"] == "ALBUM" ? "true" : "false";
            document.getElementById("gallery-image-result").innerHTML +=
                '<div class="card" onclick="javascript:ctr_gallery.goTo(' + is_album + ",'" + ctr_gallery.currentImageResult[i]["dir"] + '\')"><img src="' + current_th + 
                '" class="card-img-top"><div class="card-body"><h5 class="card-title">' + ctr_gallery.currentImageResult[i]["title"] +
                '</h5><p class="card-text">' + (ctr_gallery.currentImageResult[i]["detail"] ? ctr_gallery.currentImageResult[i]["detail"] : "") + '</p></div>';
        }
    },
    showPagination: function(total_page, current_page) {
        var html_obj = document.getElementsByClassName("gallery-pagination");
        for (var j = 0; j < html_obj.length; j++){
            html_obj[j].innerHTML = "";
        }
        for (var i = 1; i <= total_page; i++) {
            for (var j = 0; j < html_obj.length; j++){
                html_obj[j].innerHTML += 
                    '<li class="page-item"><a class="page-link" href="javascript:ctr_gallery.processPage(' + i + ')">' + i + '</a></li>';
            }
        }
    },
    goTo : function(is_album, dir) {
        if (is_album) {
            ctr_gallery.currentPath += dir + "/";
            ctr_gallery.processPage(1);
        } else {
            ctr_gallery.processView(ctr_gallery.currentPath + dir);
            ctr_gallery.currentImageName = dir;
        }
    },
    downloadImage : function() {
        SaveBlob(document.getElementById('gallery-image').src, ctr_gallery.currentImageName);
    },
    showImagePopup: function() {
        ShowImagePopup(document.getElementById('gallery-image').src);
    },
    showModal: async function() {
        document.getElementById("gallery-modal-detail").innerHTML = "";
        var preq = JSON.parse(await RequestXhrGetPromise("exif?dir=" + ctr_gallery.currentPath + ctr_gallery.currentImageName));
        if (preq["status"]) {
            for (const [key, value] of Object.entries(preq["result"])) {
                document.getElementById("gallery-modal-detail").innerHTML += "<li>" + key + " : " + value + "</li>";
            }
        } else {
            document.getElementById("gallery-modal-detail").innerHTML = "EXIF 정보를 읽을 수 없습니다."
        }
        document.getElementById("gallery-modal").classList.remove("gallery-hide");
    },
    hidePopup: function() {
        document.getElementById('gallery-popup').classList.add('gallery-hide');
        document.getElementById('gallery-modal').classList.add('gallery-hide');
    },
    makeImageBlob : async function (dir) {
        var preq = JSON.parse(await RequestXhrGetPromise("image?dir=" + dir));
        if (preq["status"]) {
            return window.URL.createObjectURL(b64toBlob(preq["image"], "data:image"));
        } else {
            console.error("Fail to get image :", preq)
            return null;
        }
    },
    makeThumbnailBlob : async function (id) {
        var preq = JSON.parse(await RequestXhrGetPromise("thumb?id=" + id));
        if (preq["status"]) {
            return window.URL.createObjectURL(b64toBlob(preq["image"], "data:image"));
        } else {
            console.error("Fail to get thumbnail :", preq);
            return null;
        }
    }
}
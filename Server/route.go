package main

import (
	"encoding/base64"
	"fmt"
	"github.com/gorilla/sessions"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo-contrib/session"
	"github.com/rwcarlsen/goexif/exif"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
	"os"
	"path"
	"strconv"
	"strings"
	"time"
	"unicode/utf8"
)

type RequestCheckParameter int

const (
	NEED_ASSIGNED_SESSION   RequestCheckParameter = 0
	NEED_UNEMPTY_CREDENTIAL                       = 1
	NEED_EMPTY_CREDENTIAL                         = 2
	NEED_POST_REQUEST_DATA                        = 4
	NEED_VALID_USER                               = NEED_ASSIGNED_SESSION | NEED_UNEMPTY_CREDENTIAL | NEED_POST_REQUEST_DATA
)

func checkRequest(cxt echo.Context, params RequestCheckParameter) (bool, map[string]interface{}, map[string]interface{}) {
	if params&NEED_ASSIGNED_SESSION == NEED_ASSIGNED_SESSION {
		if !hasSession(cxt) {
			return false, map[string]interface{}{"status": false, "error": "INVALID_SESSION"}, nil
		}
	}
	if unemcr, emcr := (params&NEED_UNEMPTY_CREDENTIAL == NEED_UNEMPTY_CREDENTIAL), (params&NEED_EMPTY_CREDENTIAL == NEED_EMPTY_CREDENTIAL); unemcr || emcr {
		ctype := getSessionValue(cxt, "credential_type")
		if ctype == "empty" && unemcr {
			return false, map[string]interface{}{"status": false, "error": "IMPROPER_CREDENTIAL"}, nil
		} else if ctype != "empty" && emcr {
			return false, map[string]interface{}{"status": false, "error": "IMPROPER_CREDENTIAL"}, nil
		}
	}
	if params&NEED_POST_REQUEST_DATA == NEED_POST_REQUEST_DATA {
		if res, reqdata := getPostRequestData(cxt); !res {
			return false, map[string]interface{}{"status": false, "error": "INVALID_REQUEST"}, nil
		} else {
			return true, nil, reqdata
		}
	}
	return true, nil, nil
}

func setRounting(e *echo.Echo) {
	//routing
	/*main_server.POST("/info")
	main_server.POST("/auth/kakao/try")
	main_server.GET("/auth/kakao/register")
	*/
	e.GET("/", func(cxt echo.Context) error {
		return cxt.JSON(http.StatusOK, map[string]interface{}{"name": "HGAL API Server", "version": serverVersion})
	})
	e.GET("/session/check", rCheckSession)
	e.GET("/session/assign", rAssignSession)
	e.POST("/auth/disp/try", rTryDisposableAuth)
	e.GET("/list", rGetImageList)
	e.GET("/image", rGetImage)
	e.GET("/thumb", rGetThumb)
	e.GET("/exif", rGetExif)
	e.Static("/web", "../Web")
}

func rGetImageList(cxt echo.Context) error {
	log.Println(makeLogPrefix(cxt, "rGetImageList"))
	if res, errdata, _ := checkRequest(cxt, NEED_ASSIGNED_SESSION|NEED_UNEMPTY_CREDENTIAL); !res {
		return cxt.JSON(http.StatusOK, errdata)
	} else {
		if val := cxt.QueryParam("dir"); val != "" {
			nowpath := imagePath + path.Clean(val)
			dirlist, err := readFileLines(nowpath + "/imglist.lst")
			if err != nil {
				log.Println(makeLogPrefix(cxt, "rGetImageList"), err)
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
			} else {
				var reslist []interface{}
				for _, nowline := range dirlist {
					nowprop := strings.Split(nowline, ",")
					if nowprop[0] == "AUTOPHOTO" {
						files, err := ioutil.ReadDir(nowpath)
						if err != nil {
							log.Println(makeLogPrefix(cxt, "rGetImageList"), err)
							return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
						} else {
							for _, file := range files {
								if !isImageFile(file.Name()) {
									continue
								}
								name, err := getThumbnailName(nowpath + "/" + file.Name())
								if err != nil {
									log.Println(makeLogPrefix(cxt, "rGetImageList"), err)
								} else {
									reslist = append(reslist, map[string]interface{}{"type": "PHOTO", "dir": file.Name(), "title": file.Name(), "detail": nil, "thimg": name, "isautoth": true})
								}
							}
						}
					} else {
						if len(nowprop) >= 5 {
							reslist = append(reslist, map[string]interface{}{"type": nowprop[0], "dir": nowprop[1], "title": nowprop[2], "detail": nowprop[3], "thimg": nowprop[4], "isautoth": false})
						} else {
							log.Println(makeLogPrefix(cxt, "rGetImageList"), "Parse error : ", nowline)
						}
					}
				}
				log.Println(makeLogPrefix(cxt, "rGetImageList"), "Image list responsed! →", len(reslist))
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true, "result": reslist})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		}
	}
	return cxt.JSON(http.StatusInternalServerError, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rGetImage(cxt echo.Context) error {
	log.Println(makeLogPrefix(cxt, "rGetImage"))
	if res, errdata, _ := checkRequest(cxt, NEED_ASSIGNED_SESSION|NEED_UNEMPTY_CREDENTIAL); !res {
		return cxt.JSON(http.StatusOK, errdata)
	} else {
		if val := cxt.QueryParam("dir"); val != "" {
			nowpath := path.Clean(val)
			imgdata, err := ioutil.ReadFile(imagePath + nowpath)
			if err != nil {
				log.Println(makeLogPrefix(cxt, "rGetImage"), err)
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
			} else {
				b64imgdata := base64.StdEncoding.EncodeToString(imgdata)
				log.Println(makeLogPrefix(cxt, "rGetImage"), "Image responsed! →", len(b64imgdata))
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true, "image": b64imgdata})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		}
	}
	return cxt.JSON(http.StatusInternalServerError, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rGetExif(cxt echo.Context) error {
	log.Println(makeLogPrefix(cxt, "rGetExif"))
	if res, errdata, _ := checkRequest(cxt, NEED_ASSIGNED_SESSION|NEED_UNEMPTY_CREDENTIAL); !res {
		return cxt.JSON(http.StatusOK, errdata)
	} else {
		if val := cxt.QueryParam("dir"); val != "" {
			nowpath := path.Clean(val)
			imgdata, err := os.Open(imagePath + nowpath)
			if err != nil {
				log.Println(makeLogPrefix(cxt, "rGetExif"), err)
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
			} else {
				exifdata, err1 := exif.Decode(imgdata)
				if err1 != nil {
					return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "DECODE_ERROR"})
				}

				//사진 너비 (ImageWidth) 사진 높이 (ImageHeight) 촬영 일시 (DateTime) 카메라 모델 (Model) 플래시 (Flash) GPS 좌표
				var exif_res = [2][6]string{
					{"사진 너비", "사진 높이", "촬영 시간", "카메라 모델", "플래시 여부", "GPS"}, //exif 데이터 의사 설명 문자열
					{"", "", "", "", "", ""}, //exif 데이터 결과 값
				}
				var err error

				width, height, err2 := getImageDimension(imagePath + nowpath)
				if err2 != nil {
					exif_res[1][0] = "알 수 없음"
					exif_res[1][1] = "알 수 없음"
					log.Println(makeLogPrefix(cxt, "rGetExif"), "Image size getting error :", err2)
				} else {
					exif_res[1][0] = strconv.FormatInt(int64(width), 10) + "px"
					exif_res[1][1] = strconv.FormatInt(int64(height), 10) + "px"
				}

				exif_res[1][3] = getExifData(exifdata, exif.Model)
				exif_res[1][4] = getExifData(exifdata, exif.Flash)

				exif_time, err := exifdata.DateTime()
				if err != nil {
					exif_res[1][2] = "알 수 없음"
				} else {
					exif_res[1][2] = exif_time.Format("2006-01-02 15:04:05")
				}

				exif_lat, exif_long, err := exifdata.LatLong()
				if err != nil {
					exif_res[1][5] = "알 수 없음"
				} else {
					exif_res[1][5] = fmt.Sprintf("%.2f", exif_lat) + " / " + fmt.Sprintf("%.2f", exif_long)
				}

				var result = make(map[string]interface{})
				for i := 0; i < len(exif_res[0]); i++ {
					result[exif_res[0][i]] = exif_res[1][i]
				}
				log.Println(makeLogPrefix(cxt, "rGetExif"), "Exif responsed! →", len(result))
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true, "result": result})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		}
	}
	return cxt.JSON(http.StatusInternalServerError, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rGetThumb(cxt echo.Context) error {
	log.Println(makeLogPrefix(cxt, "rGetThumb"))
	if res, errdata, _ := checkRequest(cxt, NEED_ASSIGNED_SESSION|NEED_UNEMPTY_CREDENTIAL); !res {
		return cxt.JSON(http.StatusOK, errdata)
	} else {
		if val := cxt.QueryParam("id"); val != "" {
			nowid := path.Clean(val)
			imgdata, err := ioutil.ReadFile(thumbnailPath + "/" + nowid + ".jpg")
			if err != nil {
				log.Println(makeLogPrefix(cxt, "rGetThumb"), err)
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
			} else {
				b64imgdata := base64.StdEncoding.EncodeToString(imgdata)
				log.Println(makeLogPrefix(cxt, "rGetThumb"), "Thumbnail responsed! →", len(b64imgdata))
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true, "image": b64imgdata})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		}
	}
	return cxt.JSON(http.StatusInternalServerError, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rTryDisposableAuth(cxt echo.Context) error {
	log.Println(makeLogPrefix(cxt, "rTryDisposableAuth"))
	if res, errdata, reqdata := checkRequest(cxt, NEED_ASSIGNED_SESSION|NEED_POST_REQUEST_DATA|NEED_EMPTY_CREDENTIAL); !res {
		return cxt.JSON(http.StatusOK, errdata)
	} else {
		var nameok, codeok bool = false, false
		var resname string

		//Requested name validation check
		if name, err := reqdata["name"].(string); !err {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		} else {
			if dname, err := url.QueryUnescape(name); (err == nil) && (utf8.RuneCountInString(dname) <= 5) {
				nameok = true
				resname = dname
			}
		}

		//Requsted code validation and correct check
		if code, err := reqdata["code"]; !err {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		} else {
			var configCode []string = convertInterfaceArrToStringArr(config["allow_disp_code"])
			for _, nowcode := range configCode {
				if code.(string) == nowcode {
					codeok = true
				}
			}
		}

		//Assign disposable auth or Response error message
		if codeok && nameok {
			setSessionValue(cxt, "credential_type", "disp")
			setSessionValue(cxt, "user_name", resname)
			log.Println(makeLogPrefix(cxt, "rTryDisposableAuth"), "Disposable auth issued! :", resname)
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true})
		} else if !nameok {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_NAME"})
		} else if !codeok {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "INCORRECT_CODE"})
		}
	}
	return cxt.JSON(http.StatusInternalServerError, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rCheckSession(cxt echo.Context) error {
	hassess := hasSession(cxt)
	log.Println(makeLogPrefix(cxt, "rCheckSession"), hassess)
	if !hassess {
		return cxt.JSON(http.StatusOK, map[string]interface{}{"is_new": true, "error": "NOT_ASSIGNED_SESSION"})
	} else {
		return cxt.JSON(http.StatusOK, map[string]interface{}{"is_new": false, "status": getSessionValue(cxt, "credential_type"), "name": getSessionValue(cxt, "user_name")})
	}
}

//이 함수는 세션을 직접 다루는게 효율적이라 래퍼 함수 사용하지 않음
func rAssignSession(cxt echo.Context) error {
	sess, _ := session.Get("session", cxt)
	_, exist := sess.Values["credential_type"]
	if !exist {
		sess.Options = &sessions.Options{
			Path:     "/",
			MaxAge:   86400 * 7, //7 days
			HttpOnly: false,
			Secure:   false,
		}
		sess.Values["assigned_time"] = time.Now().Format(time.RFC3339)
		sess.Values["credential_type"] = "empty"
		sess.Values["user_name"] = ""
	}
	sess.Save(cxt.Request(), cxt.Response())
	log.Println(makeLogPrefix(cxt, "rAssignSession"), exist)
	return cxt.JSON(http.StatusOK, map[string]interface{}{"is_new": !exist, "assigned_time": sess.Values["assigned_time"].(string)})
}

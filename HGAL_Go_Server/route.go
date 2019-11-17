package main

import (
	"encoding/base64"
	"github.com/gorilla/sessions"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
	"path"
	"strings"
	"time"
	"unicode/utf8"
)

type RequestCheckParameter int

const (
	NEED_ASSIGNED_SESSION RequestCheckParameter = 0 + 2*iota
	NEED_VALID_CREDENTIAL
	NEED_VALID_REQUEST_DATA
	NEED_ALL = NEED_ASSIGNED_SESSION | NEED_VALID_CREDENTIAL | NEED_VALID_REQUEST_DATA
)

func checkRequest(cxt echo.Context, params RequestCheckParameter) (bool, map[string]interface{}, map[string]interface{}) {
	if params&NEED_ASSIGNED_SESSION == NEED_ASSIGNED_SESSION {
		if !hasSession(cxt) {
			return false, map[string]interface{}{"status": false, "error": "INVALID_SESSION"}, nil
		}
	}
	if params&NEED_VALID_CREDENTIAL == NEED_VALID_CREDENTIAL {
		if getSessionValue(cxt, "credential_type") == "empty" {
			return false, map[string]interface{}{"status": false, "error": "IMPROPER_CREDENTIAL"}, nil
		}
	}
	if params&NEED_VALID_REQUEST_DATA == NEED_VALID_REQUEST_DATA {
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
	main_server.GET("/thumb")*/
	e.GET("/", func(cxt echo.Context) error {
		return cxt.JSON(http.StatusOK, map[string]interface{}{"name": "HGAL API Server", "version": "191117"})
	})
	e.GET("/session/check", rCheckSession)
	e.GET("/session/assign", rAssignSession)
	e.POST("/auth/disp/try", rTryDisposableAuth)
	e.GET("/list", rGetImageList)
	e.GET("/image", rGetImage)
	e.Static("/web", "../Web")
}

func rGetImageList(cxt echo.Context) error {
	if res, errdata, _ := checkRequest(cxt, NEED_ASSIGNED_SESSION|NEED_VALID_CREDENTIAL); !res {
		return cxt.JSON(http.StatusOK, errdata)
	} else {
		if val := cxt.QueryParam("dir"); val != "" {
			nowpath := execute_path + "/image" + path.Clean(val)
			dirlist, err := readFileLines(nowpath + "imglist.lst")
			if err != nil {
				log.Println("[rGetImageList]", err)
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
			} else {
				var reslist []interface{}
				for _, nowline := range dirlist {
					nowprop := strings.Split(nowline, ",")
					if nowprop[0] == "AUTOPHOTO" {
						files, err := ioutil.ReadDir(nowpath)
						if err != nil {
							log.Println("[rGetImageList]", err)
							return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
						} else {
							for _, file := range files {
								if !isImageFile(file.Name()) {
									continue
								}
								name, err := getThumbnailName(nowpath + file.Name())
								if err != nil {
									log.Println("[rGetImageList]", err)
								} else {
									reslist = append(reslist, map[string]interface{}{"type": "PHOTO", "dir": file.Name(), "title": file.Name(), "detail": nil, "thimg": name, "isautoth": true})
								}
							}
						}
					} else {
						reslist = append(reslist, map[string]interface{}{"type": nowprop[0], "dir": nowprop[1], "title": nowprop[2], "detail": nowprop[3], "thimg": nowprop[4], "isautoth": false})
					}
				}
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true, "result": reslist})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		}
	}
	return cxt.JSON(http.StatusInternalServerError, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rGetImage(cxt echo.Context) error {
	if res, errdata, _ := checkRequest(cxt, NEED_ASSIGNED_SESSION&NEED_VALID_CREDENTIAL); !res {
		return cxt.JSON(http.StatusOK, errdata)
	} else {
		if val := cxt.QueryParam("dir"); val != "" {
			nowpath := path.Clean(val)
			imgdata, err := ioutil.ReadFile(execute_path + "/image" + nowpath)
			if err != nil {
				log.Println("[rGetImage]", err)
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "PATH_INVALID"})
			} else {
				b64imgdata := base64.StdEncoding.EncodeToString(imgdata)
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true, "image": b64imgdata})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
		}
	}
	return cxt.JSON(http.StatusInternalServerError, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rTryDisposableAuth(cxt echo.Context) error {
	if succ, reqdata := getPostRequestData(cxt); succ {
		if hassess := hasSession(cxt); hassess {
			if getSessionValue(cxt, "credential_type") == "empty" {
				var nameok, codeok bool = false, false
				var resname string

				//Requested name validation check
				if name, err := reqdata["name"].(string); !err {
					return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
				} else {
					if dname, err := url.QueryUnescape(name); err == nil {
						if utf8.RuneCountInString(dname) <= 5 {
							nameok = true
							resname = dname
						} else {
							return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_NAME"})
						}
					} else {
						return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_NAME"})
					}
				}

				//Requsted code validation and correct check
				if _, err := reqdata["code"]; !err {
					return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_REQUEST"})
				} else {
					for _, nowcode := range allow_dispauth_code {
						if reqdata["code"].(string) == nowcode {
							codeok = true
						}
					}
					if !codeok {
						return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "INCORRECT_CODE"})
					}
				}

				//All verify process passed, Assign disposable auth
				if codeok && nameok {
					setSessionValue(cxt, "credential_type", "disp")
					setSessionValue(cxt, "user_name", resname)
					return cxt.JSON(http.StatusOK, map[string]interface{}{"status": true})
				}
			} else {
				return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "ILLEGAL_CREDENTIAL"})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "INVALID_SESSION"})
		}
	} else {
		return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "INVALID_REQUEST"})
	}
	return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation, code must not be reach here
}

func rCheckSession(cxt echo.Context) error {
	hassess := hasSession(cxt)
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
	return cxt.JSON(http.StatusOK, map[string]interface{}{"is_new": !exist, "assigned_time": sess.Values["assigned_time"].(string)})
}
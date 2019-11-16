package main

import (
	"github.com/gorilla/sessions"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"net/http"
	"net/url"
	"strconv"
	"time"
	"unicode/utf8"
)

func rGetImageList(cxt echo.Context) error {
	return cxt.NoContent(http.StatusOK)
}

func rGetImage(cxt echo.Context) error {
	if succ, reqdata := getRequestData(cxt); succ {
		if hassess := hasSession(cxt); hassess {
		}
	} else {
		return cxt.JSON(http.StatusOK, map[string]interface{}{"status": false, "error": "INVALID_REQUEST"})
	}
	return cxt.NoContent(http.StatusOK)
}

func rTryDisposableAuth(cxt echo.Context) error {
	if succ, reqdata := getRequestData(cxt); succ {
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
	return cxt.JSON(http.StatusOK, map[string]string{"is_new": strconv.FormatBool(!exist), "assigned_time": sess.Values["assigned_time"].(string)})
}

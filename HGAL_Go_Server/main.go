package main

import (
	"github.com/gorilla/sessions"
	"github.com/kardianos/osext"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
	"strconv"
	"strings"
	"time"
	"unicode/utf8"
)

var execute_path string
var allow_dispauth_code []string

func checkError(err error) {
	if err != nil {
		log.Fatal(err)
		panic(err)
	}
}

func main() {
	execute_path, err := osext.ExecutableFolder()
	checkError(err)
	dispauthfile, err := ioutil.ReadFile(execute_path + "/allowDispAuthCode.txt")
	checkError(err)
	allow_dispauth_code = strings.Split(string(dispauthfile), "\n")

	main_server := echo.New()
	main_server.Use(session.Middleware(sessions.NewFilesystemStore("", []byte("<TEST SECRET!!>"))))

	//routing
	/*main_server.POST("/info")
	main_server.POST("/auth/kakao/try")
	main_server.GET("/auth/kakao/register")

	main_server.GET("/auth/disp/register")
	main_server.GET("/list")
	main_server.GET("/image")
	main_server.GET("/thumb")*/
	main_server.GET("/", func(cxt echo.Context) error {
		return cxt.String(http.StatusOK, "HGAL API Server")
	})
	main_server.GET("/session/check", rCheckSession)
	main_server.GET("/session/assign", rAssignSession)
	main_server.POST("/auth/disp/try", rTryDisposableAuth)

	main_server.Logger.Fatal(main_server.Start(":80"))
}

func rTryDisposableAuth(cxt echo.Context) error {
	hassess := hasSession(cxt)
	reqdata := echo.Map{}
	if err := cxt.Bind(&reqdata); err != nil {
		log.Print("Unexpected request bind error! : ")
		log.Println(err)
		return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "INVALID_REQUEST"})
	}

	if hassess {
		if getSessionValue(cxt, "credential_type") == "empty" {
			var nameok, codeok bool = false, false
			var resname string

			//Requested name validation check
			if name, err := reqdata["name"].(string); !err {
				return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "ILLEGAL_REQUEST"})
			} else {
				if dname, err := url.QueryUnescape(name); err == nil {
					if utf8.RuneCountInString(dname) <= 5 {
						nameok = true
						resname = dname
					} else {
						return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "ILLEGAL_NAME"})
					}
				} else {
					return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "ILLEGAL_NAME"})
				}
			}

			//Requsted code validation and correct check
			if _, err := reqdata["code"]; !err {
				return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "ILLEGAL_REQUEST"})
			} else {
				for _, nowcode := range allow_dispauth_code {
					if reqdata["code"] == nowcode {
						codeok = true
					}
				}
				if !codeok {
					return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "INCORRECT_CODE"})
				}
			}

			//All verify process passed, Assign disposable auth
			if codeok && nameok {
				setSessionValue(cxt, "credential_type", "disp")
				setSessionValue(cxt, "user_name", resname)
				return cxt.JSON(http.StatusOK, map[string]string{"status": "true"})
			}
		} else {
			return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "ILLEGAL_CREDENTIAL"})
		}
	} else {
		return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "INVALID_SESSION"})
	}
	return cxt.JSON(http.StatusOK, map[string]string{"status": "false", "error": "INTERNAL_SERVER_ERROR"}) //When all expected situation must not be reach here
}

func rCheckSession(cxt echo.Context) error {
	hassess := hasSession(cxt)
	if !hassess {
		return cxt.JSON(http.StatusOK, map[string]string{"isNew": "true", "error": "NOT_ASSIGNED_SESSION"})
	} else {
		return cxt.JSON(http.StatusOK, map[string]string{"isNew": "false", "status": getSessionValue(cxt, "credential_type"), "name": getSessionValue(cxt, "user_name")})
	}
}

//이 함수는 세션을 직접 다루는게 효율적이라 래퍼 함수 사용하지 말기
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

func hasSession(cxt echo.Context) bool {
	sess, _ := session.Get("session", cxt)
	_, exist := sess.Values["credential_type"]
	return exist
}

func getSessionValue(cxt echo.Context, key string) string {
	sess, _ := session.Get("session", cxt)
	val, _ := sess.Values[key]
	sess.Save(cxt.Request(), cxt.Response())
	return val.(string)
}

func setSessionValue(cxt echo.Context, key string, val string) {
	sess, _ := session.Get("session", cxt)
	sess.Values[key] = val
	sess.Save(cxt.Request(), cxt.Response())
}

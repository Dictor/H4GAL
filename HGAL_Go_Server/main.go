package main

import (
	"github.com/gorilla/sessions"
	"github.com/kardianos/osext"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"io/ioutil"
	"log"
	"net/http"
	"strings"
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
	main_server.GET("/thumb")*/
	main_server.GET("/", func(cxt echo.Context) error {
		return cxt.String(http.StatusOK, "HGAL API Server")
	})
	main_server.GET("/session/check", rCheckSession)
	main_server.GET("/session/assign", rAssignSession)
	main_server.POST("/auth/disp/try", rTryDisposableAuth)
	main_server.GET("/list", rGetImageList)
	main_server.GET("/image", rGetImage)
	main_server.Static("/web", "../Web")
	main_server.Logger.Fatal(main_server.Start(":80"))
}

func getRequestData(cxt echo.Context) (bool, map[string]interface{}) {
	res := make(map[string]interface{})
	if err := cxt.Bind(&res); err != nil {
		log.Print("Unexpected request bind error! : ", err)
		return false, nil
	} else {
		return true, res
	}
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

package main

import (
	"github.com/gorilla/sessions"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"net/http"
	"strconv"
	"time"
)

func main() {
	var main_server = echo.New()
	main_server.Use(session.Middleware(sessions.NewFilesystemStore("", []byte("<TEST SECRET!!>"))))

	//routing
	/*main_server.POST("/info")
	main_server.POST("/auth/kakao/try")
	main_server.GET("/auth/kakao/register")
	main_server.POST("/auth/disp/try")
	main_server.GET("/auth/disp/register")
	main_server.GET("/list")
	main_server.GET("/image")
	main_server.GET("/thumb")*/
	main_server.GET("/", func(cxt echo.Context) error {
		return cxt.String(http.StatusOK, "Hello, World!")
	})
	main_server.GET("/session/check", rCheckSession)
	main_server.GET("/session/assign", rAssignSession)

	main_server.Logger.Fatal(main_server.Start(":80"))
}

func rCheckSession(cxt echo.Context) error {
	sess, _ := session.Get("session", cxt)
	_, exist := sess.Values["credential_type"]
	sess.Save(cxt.Request(), cxt.Response())
	if !exist {
		return cxt.JSON(http.StatusOK, map[string]string{"isNew": strconv.FormatBool(!exist), "error": "NOT_ASSIGNED_SESSION"})
	} else {
		return cxt.JSON(http.StatusOK, map[string]string{"isNew": strconv.FormatBool(!exist), "status": sess.Values["credential_type"].(string), "name": sess.Values["user_name"].(string)})
	}
}

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

package main

import (
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"log"
)

//cxt를 포인터로 받는게 낫지 않나??
func getPostRequestData(cxt echo.Context) (bool, map[string]interface{}) {
	res := make(map[string]interface{})
	if err := cxt.Bind(&res); err != nil {
		log.Println("[getRequestData]", err)
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

func makeLogPrefix(cxt echo.Context, func_name string) string {
	id := cxt.Request().Header.Get(echo.HeaderXRequestID)
	if id == "" {
		id = cxt.Response().Header().Get(echo.HeaderXRequestID)
	}
	var params = [...]string{func_name, id, cxt.RealIP(), cxt.Path() + "?" + cxt.QueryString()}
	var result string
	for _, val := range params {
		result += "[" + val + "]"
	}
	return result
}

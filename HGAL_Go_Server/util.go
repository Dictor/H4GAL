package main

import (
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"io/ioutil"
	"log"
	"os"
	"strings"
)

var allow_image_extension = [...]string{"jpg", "jpeg", "png", "bmp", "gif"}

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

func readFileLines(path string) ([]string, error) {
	data, err := ioutil.ReadFile(path)
	if err != nil {
		return nil, err
	}
	return strings.Split(string(data), "\n"), nil
}

func isImageFile(filename string) bool {
	pname := strings.Split(filename, ".")
	res := false
	for _, ext := range allow_image_extension {
		if ext == pname[len(pname)-1] {
			res = true
		}
	}
	return res
}

func checkFileExist(filename string) bool {
	info, err := os.Stat(filename)
	if os.IsNotExist(err) {
		return false
	}
	return !info.IsDir()
}

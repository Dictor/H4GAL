package main

import (
	"github.com/gorilla/sessions"
	"github.com/kardianos/osext"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"io/ioutil"
	"log"
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
	var err error
	execute_path, err = osext.ExecutableFolder()
	checkError(err)
	log.Println("Executing path is ", execute_path)
	dispauthfile, err := ioutil.ReadFile(execute_path + "/allowDispAuthCode.txt")
	checkError(err)
	allow_dispauth_code = strings.Split(string(dispauthfile), "\n")

	main_server := echo.New()
	main_server.Use(session.Middleware(sessions.NewFilesystemStore("", []byte("<TEST SECRET!!>"))))
	setRounting(main_server)
	main_server.Logger.Fatal(main_server.Start(":80"))
}

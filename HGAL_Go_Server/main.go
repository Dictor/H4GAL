package main

import (
	"github.com/gorilla/sessions"
	"github.com/kardianos/osext"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"log"
)

var executionPath, thumbnailPath, imagePath string
var allowDispAuthCode []string

const serverVersion string = "191122"

func checkError(err error) {
	if err != nil {
		log.Panic(err)
	}
}

func main() {
	var err error
	executionPath, err = osext.ExecutableFolder()
	checkError(err)
	allowDispAuthCode, err = readFileLines(executionPath + "/allowDispAuthCode.txt")
	checkError(err)
	thumbnailPath = executionPath + "/image"
	imagePath = executionPath + "/thumb"
	thok, thfail, err := makeRecursiveThumnail(thumbnailPath, imagePath)
	checkError(err)

	log.Println("\n------ HGAL API Server successfully initialized!!\n* Execution path :",
		executionPath, "\n* Thumbnail path :",
		thumbnailPath, "\n* Image path :",
		imagePath, "\n* Version :",
		serverVersion, "\n* Thumbnail made :",
		thok, "\n* Thumbnail fail :",
		thfail)

	main_server := echo.New()
	main_server.Use(session.Middleware(sessions.NewFilesystemStore("", []byte("<TEST SECRET!!>"))))
	setRounting(main_server)
	main_server.Logger.Fatal(main_server.Start(":80"))
}

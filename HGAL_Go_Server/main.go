package main

import (
	"github.com/gorilla/sessions"
	"github.com/kardianos/osext"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"log"
)

var executionPath, thumbnailPath, imagePath, logPath string
var allowDispAuthCode []string

const serverVersion string = "191129"

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
	imagePath = executionPath + "/image"
	thumbnailPath = executionPath + "/thumb"
	logPath = executionPath + "/log"
	prepareDirectory(imagePath, thumbnailPath, logPath)
	thok, thfail, err := makeRecursiveThumnail(imagePath, thumbnailPath)
	checkError(err)

	log.Println("\n------ HGAL API Server successfully initialized!!\n* Execution path :\t",
		executionPath, "\n* Thumbnail path :\t",
		thumbnailPath, "\n* Image path :\t\t",
		imagePath, "\n* Version :\t\t",
		serverVersion, "\n* Thumbnail made :\t",
		thok, "\n* Thumbnail fail :\t",
		thfail)

	main_server := echo.New()
	main_server.Use(session.Middleware(sessions.NewFilesystemStore("", []byte("<TEST SECRET!!>"))))
	setRounting(main_server)
	main_server.Logger.Fatal(main_server.Start(":80"))
}

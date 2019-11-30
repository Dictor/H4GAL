package main

import (
	"github.com/gorilla/sessions"
	"github.com/kardianos/osext"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"io"
	"log"
	"os"
	"time"
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

	fpLog, err := os.OpenFile(logPath+"/"+time.Now().Format("2006-01-02T15:04:05")+".txt", os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0666)
	checkError(err)
	defer fpLog.Close()
	multiWriter := io.MultiWriter(fpLog, os.Stdout)
	log.SetOutput(multiWriter)

	thok, thfail, err := makeRecursiveThumnail(imagePath, thumbnailPath)
	checkError(err)

	log.Println("\n------ HGAL API Server successfully initialized!!\n* Execution path :\t",
		executionPath, "\n* Thumbnail path :\t",
		thumbnailPath, "\n* Image path :\t\t",
		imagePath, "\n* Log path :\t\t",
		logPath, "\n* Version :\t\t",
		serverVersion, "\n* Thumbnail made :\t",
		thok, "\n* Thumbnail fail :\t",
		thfail)

	main_server := echo.New()
	main_server.Use(session.Middleware(sessions.NewFilesystemStore("", []byte("<TEST SECRET!!>"))))
	setRounting(main_server)
	main_server.Logger.Fatal(main_server.Start(":80"))
}

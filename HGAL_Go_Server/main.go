package main

import (
	"encoding/json"
	"github.com/gorilla/sessions"
	"github.com/kardianos/osext"
	"github.com/labstack/echo"
	"github.com/labstack/echo-contrib/session"
	"github.com/labstack/echo/middleware"
	"github.com/labstack/gommon/random"
	"io"
	"io/ioutil"
	"log"
	"os"
	"time"
)

var executionPath, thumbnailPath, imagePath, logPath, sessionPath string
var config map[string]interface{}

const serverVersion string = "191202-1"

func checkError(err error) {
	if err != nil {
		log.Panic(err)
	}
}

func main() {
	var err error

	executionPath, err = osext.ExecutableFolder()
	checkError(err)

	var configRaw []byte
	configRaw, err = ioutil.ReadFile(executionPath + "/config.json")
	checkError(err)
	err = json.Unmarshal(configRaw, &config)
	checkError(err)

	imagePath = executionPath + "/image"
	thumbnailPath = executionPath + "/thumb"
	logPath = executionPath + "/log"
	sessionPath = executionPath + "/session"
	prepareDirectory(imagePath, thumbnailPath, logPath, sessionPath)

	fpLog, err := os.OpenFile(logPath+"/"+time.Now().Format("2006-01-02T15_04_05")+".txt", os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0666)
	checkError(err)
	defer fpLog.Close()
	multiWriter := io.MultiWriter(fpLog, os.Stdout)
	log.SetOutput(multiWriter)

	log.Println("Thumbnail inspecting start")
	thok, thfail, err := makeRecursiveThumnail(imagePath, thumbnailPath)
	checkError(err)
	log.Println("Thumbnail inspecting complete")

	log.Println("\n------ HGAL API Server successfully initialized!!\n* Execution path :\t",
		executionPath, "\n* Thumbnail path :\t",
		thumbnailPath, "\n* Image path :\t\t",
		imagePath, "\n* Log path :\t\t",
		logPath, "→", fpLog.Name(), "\n* Version :\t\t",
		serverVersion, "\n* Thumbnail made :\t",
		thok, "\n* Thumbnail fail :\t",
		thfail)

	main_server := echo.New()
	main_server.Use(session.Middleware(sessions.NewFilesystemStore(sessionPath, []byte(config["session_secret"].(string)))))
	main_server.Use(middleware.RequestIDWithConfig(middleware.RequestIDConfig{
		Generator: func() string {
			return random.String(8)
		},
	}))
	main_server.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins:     convertInterfaceArrToStringArr(config["allow_cors_origin"]),
		AllowMethods:     []string{"GET", "POST"},
		AllowCredentials: true,
	}))
	main_server.HTTPErrorHandler = func(err error, cxt echo.Context) {
		log.Println(makeLogPrefix(cxt, "HTTP_ERROR_HANDLER"), err)
		main_server.DefaultHTTPErrorHandler(err, cxt)
	}
	setRounting(main_server)
	main_server.Logger.Fatal(main_server.Start(":" + config["server_port"].(string)))
}

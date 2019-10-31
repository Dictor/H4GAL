package main

import (
	"net/http"
	"github.com/labstack/echo"
)

func main() {
	var main_server = echo.New()

	main_server.POST("/info", )	
	main_server.POST("/session/check", )
	main_server.GET("/session/assign", )
	main_server.POST("/auth/kakao/try", )
	main_server.GET("/auth/kakao/register", )
	main_server.POST("/auth/disp/try", )
	main_server.GET("/auth/disp/register", )
	main_server.GET("/list", )
	main_server.GET("/image", )
	ain_server.GET("/thumb", )
	
	main_server.Logger.Fatal(main_server.Start(":80"))
}
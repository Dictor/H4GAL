package main

import (
	"github.com/rwcarlsen/goexif/exif"
	"io/ioutil"
	"os"
	"strings"
)

var allow_image_extension = [...]string{"jpg", "jpeg", "png", "bmp", "gif"}

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

func getExifData(exifdata *exif.Exif, exifparam exif.FieldName) string {
	res, err := exifdata.Get(exifparam)
	if err != nil {
		return "알 수 없음"
	} else {
		res, _ := res.StringVal()
		return res
	}
}

func prepareDirectory(dir ...string) {
	for _, val := range dir {
		if _, err := os.Stat(val); os.IsNotExist(err) {
			os.Mkdir(val, 0666)
		}
	}
}

package main

import (
	"github.com/disintegration/imaging"
	"hash/crc32"
	"io/ioutil"
	"log"
	"os"
	"path/filepath"
	"strconv"
)

func exploreDirectory(startPath string) ([]string, error) {
	//https://golang.org/pkg/path/filepath/#WalkFunc 로 구현
	var result []string
	err := filepath.Walk(startPath, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if !info.IsDir() {
			result = append(result, path)
		}
		return nil
	})
	return result, err
}

func makeThumbnail(sourcePath string, thumbPath string, thumbSize int) error {
	//https://github.com/disintegration/imaging 로 구현
	img, err := imaging.Open(sourcePath)
	if err != nil {
		return err
	}
	img = imaging.Fill(img, thumbSize, int(float32(thumbSize)*0.76), imaging.Center, imaging.Lanczos)
	err = imaging.Save(img, thumbPath)
	return err
}

func makeRecursiveThumnail(startDir string, thumbDir string) (madecnt int, errcnt int, fatalerr error) {
	files, err := exploreDirectory(startDir)
	if err != nil {
		return madecnt, errcnt, err
	}

	for _, nowpath := range files {
		if isImageFile(nowpath) {
			name, err := getThumbnailName(nowpath)
			if err != nil {
				log.Println("[makeRecursiveThumnail]", err)
				errcnt++
			} else {
				thumbPath := thumbDir + "/" + name + ".jpg"
				if !checkFileExist(thumbPath) {
					log.Println("[makeRecursiveThumnail]", nowpath, "→", thumbPath)
					makeThumbnail(nowpath, thumbPath, 300)
					madecnt++
				}
			}
		}
	}
	return madecnt, errcnt, nil
}

func getThumbnailName(path string) (string, error) {
	img, err := ioutil.ReadFile(path)
	if err != nil {
		return "", err
	}
	csum := crc32.ChecksumIEEE(img)
	return strconv.Itoa(len(img)) + "-" + strconv.FormatUint(uint64(csum), 10), nil
}

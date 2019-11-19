package main

import (
	"github.com/disintegration/imaging"
	"os"
	"path/filepath"
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
	img = imaging.Fill(img, thumbSize, thumbSize, imaging.Center, imaging.Lanczos)
	err = imaging.Save(img, thumbPath)
	return err
}

func makeRecursiveThumnail(startPath string) error {
	//위 두함수 종합
	return nil
}

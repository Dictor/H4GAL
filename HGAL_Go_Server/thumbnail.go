package main

import (
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

func makeThumbnail(imagePath string) error {
	return nil
}

func makeRecursiveThumnail(startPath string) error {
	return nil
}

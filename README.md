# HGAL
[하늘고 4기 사진저장소](https://chinchister.com/h4gal)의 백앤드와 프론트앤드 코드 저장소입니다.


## 설정 파일 작성법
### config.json
실행파일과 같은 디렉토리에 위치시키고 아래와 같은 내용을 작성합니다.
```
{
    "allow_disp_code": ["<YOUR CODE>"],
    "allow_cors_origin": ["<YOUR FRONTEND DOMAIN>"],
    "server_port": "<YOUR PORT>",
    "session_secret": "<YOUR SESSION SECRET>"
}
```

### imglist.lst
각 앨범 폴더에 위치하여 백앤드 서버에게 앨범 구조를 알려줍니다.
```
<"ALBUM" or "PHOTO">,<ALBUM FORDER NAME>,<DISPLAYING ALBUM NAME>,<DISPLAYING ALBUM DETAIL>,<DISPLAYING THUMBNAIL ID>,ALL
```
예시
```
ALBUM,앨범,테스트 앨범,테스트 앨범입니다,0000-000000000,ALL
```
내용이 아래와 같으면 백앤드 서버가 자동으로 사진을 읽어 앨범 구조를 생성합니다. (재귀적으로 읽지 않습니다)
```
AUTOPHOTO
```

## 요구 의존성
### 백앤드
* Go 1.13 이상
* Echo v4
* github.com/gorilla/sessions
* github.com/kardianos/osext
*	github.com/labstack/gommon/random
* github.com/rwcarlsen/goexif/exif
### 프론트앤드
* ES6

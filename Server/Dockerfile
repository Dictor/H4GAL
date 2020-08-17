FROM golang:1.15

COPY . /h4gal
WORKDIR /h4gal
RUN ["go", "build"]
ENTRYPOINT ["/h4gal/h4gal"]

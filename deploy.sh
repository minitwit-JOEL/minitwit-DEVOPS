#!/usr/bin/env  bash

docker image pull :latest
docker image pull :latest
docker image pull :latest

docker stop $(docker ps -q)

docker run -d :latest
docker run -d :latest
docker run -d :latest

docker container prune -f
docker image prune
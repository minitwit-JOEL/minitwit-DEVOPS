#!/usr/bin/env  bash

docker image pull lukan707/minitwit-joel-db:latest
docker image pull lukan707/minitwit-joel-api:latest
docker image pull lukan707/minitwit-joel-web:latest

docker stop $(docker ps -q)

docker network create minitwit-network

docker container prune -f

docker run -d --mount source=minitiwt-devops_db-data,target=/var/lib/postgresql/data --network=minitwit-network -p 5432:5432 --name=minitwit-db --env-file .secrets lukan707/minitwit-joel-db:latest

#until [ "$(docker inspect -f '{{.State.Health.Status}}' minitwit-db)" == "healthy" ]; do
#  sleep 1
#  echo 'Database is not ready'
#done

sleep 60

docker run -d --network=minitwit-network -p 8080:8080 -p 8081:8081 --name=minitwit-api --env-file .secrets lukan707/minitwit-joel-api:latest
docker run -d --network=minitwit-network -p 3000:3000 --name=minitwit-web -e "API_BASE_URL=http://minitwit-api:8080/" -e "NODE_TLS_REJECT_UNAUTHORIZED=0" lukan707/minitwit-joel-web:latest 

docker image prune -f

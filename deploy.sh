#!/usr/bin/env  bash
docker image pull lukan707/minitwit-joel-api:latest
docker image pull lukan707/minitwit-joel-web:latest
docker image pull prom/prometheus:latest
docker image pull grafana/grafana:10.2.4

 SWARM_IP=$(ip -4 addr show $(ip route show default | awk '/default/ {print $5}') | awk '/inet / {print $2}' | cut -d/ -f1)

# # shellcheck disable=SC2046
# docker rm -f $(docker ps -aq)

# docker network create minitwit-network

# docker container prune -f

# if [ "$1" == "--local" ]; then
#     docker run -d --mount source=minitiwt-devops_db-data,target=/var/lib/postgresql/data --network=minitwit-network -p 5432:5432 --name=minitwit-db --env-file .secrets-local lukan707/minitwit-joel-db:latest

#     until  docker exec minitwit-db pg_isready -U postgres; do
#         sleep 1
#         echo 'Database is not ready'
#     done

#     docker run -d --network=minitwit-network -p 8080:8080 -p 8081:8081 --name=minitwit-api --env-file .secrets-local lukan707/minitwit-joel-api:latest
# else
#     docker run -d --network=minitwit-network -p 8080:8080 -p 8081:8081 --name=minitwit-api --env-file .secrets-production lukan707/minitwit-joel-api:latest
# fi

# docker run -d --network=minitwit-network -p 3000:3000 --name=minitwit-web -e "API_BASE_URL=http://minitwit-api:8080/" -e "NODE_TLS_REJECT_UNAUTHORIZED=0" lukan707/minitwit-joel-web:latest 

# until curl -f http://localhost:8080/api/sim/latest; do
#     sleep 1
#     echo 'Minitwit API is not ready'
# done

# docker run -d --network=minitwit-network -p 9090:9090 --name=prometheus -v ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus:latest
# docker run -d --network=minitwit-network -p 3001:3000 --name=grafana -v ./grafana/provisioning/dashboards:/etc/grafana/provisioning/dashboards -v ./grafana/dashboards:/var/lib/grafana/dashboards -v ./grafana/provisioning/datasources:/etc/grafana/provisioning/datasources grafana/grafana:10.2.4

# docker image prune -f 

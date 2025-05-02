#!/usr/bin/env  bash
docker image pull lukan707/minitwit-joel-api:latest
docker image pull lukan707/minitwit-joel-web:latest
docker image pull prom/prometheus:latest
docker image pull grafana/grafana:10.2.4

SWARM_IP="$(ip -4 addr show $(ip route show default | awk '/default/ {print $5}') | awk '/inet / {print $2}' | cut -d/ -f1 | head -1 )"

echo "The IP of the swarm is: $SWARM_IP"

if [ "$1" == "--local" ]; then
    docker swarm leave --force
    # Shellcheck sc2046 is disabled for the following line, since we actually want word splitting
    # shellcheck disable=SC2046
    docker rm -f $(docker ps -aq)
fi

if ! docker node inspect self --format '{{ .ManagerStatus.Leader }}'; then
    echo "Starting docker swarm"
    docker swarm init --advertise-addr "$SWARM_IP:2377"
fi

docker network create minitwit-network \
    --driver overlay \
    --attachable \

ENV_FILE=".secrets-production"

if [ "$1" == "--local" ]; then
    ENV_FILE=".secrets-local"
    
    docker rm -f minitwit-db
    
    docker run -d \
    --mount source=minitiwt-devops_db-data,target=/var/lib/postgresql/data \
    --network=minitwit-network \
    -p 5432:5432 \
    --name=minitwit-db \
    --env-file $ENV_FILE \
    lukan707/minitwit-joel-db:latest

    until  docker exec minitwit-db pg_isready -U postgres; do
        sleep 1
        echo 'Database is not ready'
    done
fi 

docker service create \
    --name minitwit-api \
    --replicas 5 \
    --network minitwit-network \
    --publish 8080:8080 \
    --publish 8081:8081 \
    --env-file $ENV_FILE \
    lukan707/minitwit-joel-api:latest

until curl -f "http://$SWARM_IP:8080/api/sim/latest"; do
    sleep 1
    echo 'Minitwit API is not ready'
done

docker service create \
    --name minitwit-web \
    --replicas 2 \
    --network minitwit-network \
    --publish 3000:3000 \
    -e "API_BASE_URL=http://minitwit-api:8080/" \
    -e "NODE_TLS_REJECT_UNAUTHORIZED=0" \
    lukan707/minitwit-joel-web:latest

docker rm -f prometheus
docker rm -f grafana

docker run -d \
    --network=minitwit-network \
    -p 9090:9090 \
    --name=prometheus \
    -v ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml \
    prom/prometheus:latest

docker run -d \
    --network=minitwit-network \
    -p 3001:3000 \
    --name=grafana \
    -v ./grafana/provisioning/dashboards:/etc/grafana/provisioning/dashboards \
    -v ./grafana/dashboards:/var/lib/grafana/dashboards \
    -v ./grafana/provisioning/datasources:/etc/grafana/provisioning/datasources \
    grafana/grafana:10.2.4

docker image prune -f 

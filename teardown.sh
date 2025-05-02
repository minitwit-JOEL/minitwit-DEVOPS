docker swarm leave --force
# Shellcheck sc2046 is disabled for the following line, since we actually want word splitting
# shellcheck disable=SC2046
docker rm -f $(docker ps -aq)
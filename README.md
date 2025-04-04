# Application Setup Guide

The application can be stared by running docker compose or by using the deploy.sh script.
Either way, the application is dependent on certain enviroment variables, which are described in the section below.

# Dependencies

Both docker compose and deploy.sh are dependen on a file called .secrets located in the root directory:

```/.secrets```

A file called appsettings.json can also be sat, if one wants to run the dotnet application without continarization.
It should have the following structure, and reside in the parth src/minitwit.Api/apisettings.json:

```sh
"ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Token": {
    "Key": "",J
    "Issuer": "",
    "Audience": ""
  },
  SimApiAccess: {
    "Key": ""
  }
```
A file called .env can also be sat, if one wants to run the node.js application without continarization.
It should have the following content, and reside in the parth src/minitwit.Web/.env:

```sh
NODE_TLS_REJECT_UNAUTHORIZED=0
API_BASE_URL=https://localhost:8081/
PORT=3100
```

# Docker compose

When it is ensured that the secrets file is in place, 
the containers can be brought up by running the following command in the root directory of the project:

**NOTICE:** this command builds the images from scratch. If the image in the corresponding Dockerfile 
or in the docker-compose.yaml file haven't been changed, it is not necesarry to build, which can take a while.

```sh
docker compose up -build
```

If no building is required:

```sh
docker compose up
```

It can also be runned in detached mode:

```sh
docker compose up -d
```

The containers can be brought down again by the command:

```sh
docker compose down
```

# Deploy.sh script

When it is ensured that the secrets file is in place, 
the containers can be brought up by running the following command in the root directory of the project:

```sh
/.deploy.sh
```

# Viewing the web application

## Localhost

The webapplication on the local enviroment can be found at <http://localhost:300>

## Production

The webapplication on the production enviroment can be found at <http://67.207.72.167:3000>

# Manually testing the simulator endpoints with a bash script

## Testing the simualtor on localhost

To test the simualtor endpoints, when standing in the root folder:

```sh
tests/./test_sim_api.sh 
```

## Testing the simualtor on production

To test the simualtor endpoints, when standing in the root folder:

```sh
tests/./test_sim_api.sh --production
```

# Viewing metrics

## Viewing metrics on localhost

The monitoring dashboard can be found at <http://localhost:9090>

## Viewing metrics on production

The monitoring dashboard can be found at

# Manually triggering the deploy.sh script

Fist ssh into the VM on which you wish to manually trigger the deploy.sh script.

Then execute the following command:

```sh
/home/github/./deploy.sh
```

# Maintainablility

## CodeClimate

The maintainablity score reported by CodeClimate can be found here:

<a href="https://codeclimate.com/github/minitwit-JOEL/minitwit-DEVOPS/maintainability"><img src="https://api.codeclimate.com/v1/badges/1a8ebed837410df38623/maintainability" /></a>
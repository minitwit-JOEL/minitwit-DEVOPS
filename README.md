# Application Setup Guide

To start the application, first **setup Docker** and **add the migrations**:

## Step 1: Install Docker

First, install Docker. To create and start the Docker container, navigate to the solution directory and run:

```sh
docker compose up -d
```
or 
```sh
docker compose up --build
```

## Step 2: Add migrations
The migrations should add themselves but if they do not, please follow below:
After this, we need to add the tables in the database. First, navigate to the Api directory:

```sh
cd src/minitwi.Api
```

Then run the following line to add the migrations:

```sh
dotnet ef database update --project ../minitwit.Infrastructure
```

## Step 4: Add .env and appsettings.json files
Now we need to add a .env file with the following contents in the root of the minitwit.Web directory:

```sh
NODE_TLS_REJECT_UNAUTHORIZED=0
API_BASE_URL=https://localhost:8081/
PORT=3100
```

Next you need to add a connection string, JWT key, issuer and audience to the appsettings.json 

```sh
"ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Token": {
    "Key": "",
    "Issuer": "",
    "Audience": ""
  }
```

## Step 5: Navigate to the web project
Next, navigate to the minitwit.Web project:

```sh
cd ../minitwi.Web
```

Then install the necessary dependencies by running:

```sh
npm install
```

## Step 6: Start the project
You are now ready to start the project. In one terminal, start the Web project by running:

```sh
npm run dev
```

And in another terminal, run the backend from the API project by running:

```sh
dotnet watch -lp https
```

Or simply use the build version that docker spins up
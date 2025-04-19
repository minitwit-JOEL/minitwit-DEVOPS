## Week 2

- Main task: Rewrite the application from Python Flask
  
We have chosen to reimplement the application as backend C# API, and a frontend Node.js server, built with the Express framework.

For the backend API we are using the AspNet.Core.Mvc package. We have controllers that contian behavouir for each endpoint, and services, which contian the database interactions, that the controllers call.

The Flask application uses server-side session storage to hold user information between HTTP requests. We have shifted to using JWT-tokens, that is send on login, and which the client sends with every request.

Instead of calling the renderer_template and sending back html, we are sending JSON to the Node.js server, which then sends html to the client.

As a small security hardening, we are no longer telling users from the API explicit, if they are entering a wrong username or password upon login, but only that one of them is wrong.

We have changed the database from Sqlite to using postgress server through Microsoft's EF Core.


## Week 4

### Implementing the simualtor API

We have implemented the simulator API endpoints, in order with given specification.
We had already factores 

### Database seems to fail after some up-time

We discovered that our database was crashing after a day or two. We tried to run the database natively, instead inside a docker contianer, but had problems getting that to work, and configuring it correctly.

After disussing our issue with Helge, we learned that the database crashed due to a lack of system ressources like ram. 
After all we tried to run a Node.js server, a dotnet server and a postgres database with 512 MB of RAM.

Upgrading the contianer to a premium AMD CPU, 2 GB of RAM and 50 GB disc space, for a monthly price of $14, solved the issue.

## Week 5

### Object relational mapping

We have already, as specified in the "Week 2" section of this logbook, switched to using ORM.

### Getting the deploy script to work

We encountered several issues while trying to make the deploy script work.
These issues were primarily centered around gconfiguring the enviroment variables correctly, as well as exposing the ports correctly.

The issues were solved by ensuring that the deploy script, started the docker containers, exactly like the docker files and the docker compose file.
We also discovered and corrected a mistake in the docker compose file, exposing the contianers port 8080 on the local systems port 8081.

Another problem we encountered, was that the API started and trying to connect and migrate on the database, before the database was ready to accept connections.
We tried defining a health check in the docker compose file, from which the images were build, and waiting with starting the api and the web container, before the database container was healthy, but without any lock.

For now, there is a temporary fix, which consists of waiting 60 seconds between starting the database container and the other containers. 
This is equivalent to the "depends on" attribute in the docker compose file.

We also encountered a problem, that when the API tried to apply migrations to the database, and thereby connection, we recieved an error, which starts with the top of the following stack trace:

```
Unhandled exception. System.Net.Sockets.SocketException (00000001, 11): Resource temporarily unavailable
   at System.Net.Dns.GetHostEntryOrAddressesCore(String hostName, Boolean justAddresses, AddressFamily addressFamily, Nullable`1 startingTimestamp)
   at System.Net.Dns.GetHostAddresses(String hostNameOrAddress, AddressFamily family)
   at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
```

The problem seemed to occur, because the two containers network could not reach each other. The implemented solution was to cerate a docker network,
and attaching every container to the network.

### Removing secrets from the files tracked by git

We have removed the api key, that our simualtor api expects out of the sim service and into appsettings.json, which is made avaliable to the sim service
through dependency injection, configured in Program.cs

We have removed the secrets out of the docker compose file, by declaring secrets through docker, and injecting them in the docker compose file, to the containers.

We have removed the secrets out of the deploy script, by declaring secrets through docker, and injecting them in the docker run commands, within the script file.

## Week 6

### Fixing the errors on the simulator api.

We have noticed that the simulator reports erros on the simulator API.

The errors reported are:
- follow
- unfollow
- tweet
- ConnectionError

We were unable to track any erros in our logs.

We have a hypothesis that the errors are coursed by the followcontroller, and that the
tweets errors are caues by missing or wrong tweets on the private timeline,
relatead to unsuccesfull posts to the follow or unfollow endpoints.

### Implementing build_and_test and release workflows

#### build_and_test.yaml

To implement the build_and_test workflow we took heavy inspiration from the one we made in the BDSA course. 
This workflow builds the API and runs tests for it, the tests have not been made yet so they are commented out. 

#### release.yaml

Again to implement this workflow we took heavy inspiration from the similar one we made in the BDSA course, although this was quite tricky to get working.

The initial problem we had was the not seeing the workflow run on github actions.
A TA helped us with this saying that we had to publish the workflow to the main branch and we could then modify it from other branches and it would still run. 

The next problem was some outdated dependencies, this includes: 
- dotnet-version  7 --> 8
- actions/checkout@  v1 --> v4
- actions/setup-dotnet@  v1 --> v4
- softprops/actions-gh-release@  v1 --> v2

Afterwards we wanted the workflow to run on tag pushes, but the tags were not being fetched correctly. 
To fix this we added checkout, with fetch-depth: 0, which seemed to resolve the issue.

After that we ran into the problem that we attempted to zip the web build to a non existant destination so we added a debug step to print all the exisisting folders. 
      - name: Debug Web Build
         working-directory: src/minitwit.Web
         run: |
           echo "Checking contents of the web build directory..."
           ls -al
This issue was solved with some help from chatGPT. 
Doing this we found that the folder we needed to zip to was a "build" folder, not a "dist" folder and we zipped correctly now. '

The last issue we ran into was that the our workflow did not have permission to write to the repo. 
To give it permissions we added a PAT to the repo, which we made the workflow use.  
  with:
           token: ${{ secrets.PERSONAL_GITHUB_TOKEN }}
           files: |
             ${{ github.workspace }}/minitwit.Api/minitwit-*.zip
             ${{ github.workspace }}/minitwit.Web/minitwit-web-*.zip
With these fixes the workflow now makes working releases. 

## Week 7

### Hotfix: The website responded with an application error when we used our CD pipeline

We discovered that after our deploy workflow had finished the deployed web application responded with an application error.

We found that when we manually build our contianers using the docker compose command, pushed the api image to the Github repository,
and manually executed our deplouy script on the production server, the application worked without errors.

By inspecting the logs we found that the error occured because the api contianer did not have the JWT-token, 
that the web application authorizes itself to the api with.

We fixed the bug by adding the JWT-token to the deploy script (and for local develop enviroment by adding it to the docker compose file).

## Week 8

### Chore: Decreasing downtime when deploying

When we created our deploy.sh script, we encountered an issue when starting the docker container with the api right after starting the contianer with the database, because the api tried to apply migrations before the the database was ready to accept connections.
We then introduced a delay between the two of a minute.

Since requests are now more frequent (several a minute), we need to decrease our downtime.

We created a loop, which checkes the database health condition (if postgres is ready), and if not, sleeps for a second, repeating this until it is ready.

We replaced the delay with this loop, such that we do not start the api contianer before the database is ready to accept connections.

### Hotfix: Multiple errors on the website

We discovered that the website had multiple errors:
- The public and private timeline were identical, except for the header
- When logged in, the ability to post a twit were missing.
- Pressing a users name in order to access the users timeline, resulted in an application error.
- Pressing logout, did not result in any action / change.

We observed that these errors did not exists on local enviroments, where our appsettings.json file were present.
This lead us to believe that the enviroment variables where to blame.

By debugging and inspecting values of the token obtained from the enviroment, we discovered that the program dit no read the token, from the .secrets file, in which enviroment variables are written in docker format.

The solution was to change the way we read the variable in Program.cs and correct the format of the .secrets file.

### Hotfix: Simulator rejecting correct API-key and latest endpoint falling behind

After updating the format of .secrets file on the production enviroment, we had made a mistake, but not formatting the simulator api key correctly.

After fixing this, the simulator accepted requests with the correct key again, and reponse form the latests endpoint, came up to date.

### Chore: Getting monitoring to work

Monitoring was quite bothersome to get working for us, for several reassons.

First and foremost there was a lot of understanding to do to just get a connection to grafana up and running. A lot of this understanding was gained by reading the relevant 
documentation, and by getting it explained by chatGPT. 

Our first issue we encountered was setting up the prometheus and grafana containers within our docker setting. From the help of TA's we learned that the prometheus and grafana yml files should target eachother within the docker network.

Once this was setup we could now connect to the grafana dashboard and start designing our dashboard. We did this but no data was being gathered and displayed on the dashboard. We researched this issue on the web and using chatGPT, and gathered that we weren't providing our dashboard with a datasource.yml or a dashboard.yml file. We then did this and observed data being displayed.

We then made a minimal dashboard that displays very basic information about the state of the site, the plan is to improve upon this. We then had to get this pushed up so we added all the relevant things to the deploy script and after a few itterations and changing the -volume tag to -v and so on we got it pushed to production and working. But we couldn't access port 3001, where our grafana dashboard runs. This was due to our firewall setup not allowing for connections on 3001, we quickly changed this and could observe the dashboard.  

### Chore: Getting linters to work


## Week 10

### chore/Testing


## Week 11

### chore/Vagrant

In order to getting vagrant to work on wsl, the following enviroment variable must be set in the unix enviroment:

```export VAGRANT_WSL_ENABLE_WINDOWS_ACCESS="1"```

Also when running the script we got the following error:

```sh
web-droplet-0: docker: Error response from daemon: failed to create task for container: failed to create shim task: OCI runtime create failed: runc create failed: unable to start container process: error during container init: error mounting "/root/prometheus/prometheus.yml" to rootfs at "/etc/prometheus/prometheus.yml": create mountpoint for /etc/prometheus/prometheus.yml mount: cannot create subdirectories in "/var/lib/docker/overlay2/930bb965980a3b024fca1f8588e9361b6c4444b17cc56b9636381e11a49d4bb9/merged/etc/prometheus/prometheus.yml": not a directory: unknown: Are you trying to mount a directory onto a file (or vice-versa)? Check if the specified host path exists and is the expected type
```

This was due to Vagrant not being able to copy folders and their content recursively when using the file provision configuration, and instead what should have been files, was created as folders.
To solve this we manually provision each file explicitly with the file provision configuration.
Before this could work, we needed to ssh into the server and manually delete the existing folders that should have been files.

Also note that the files are neither copied into the root- or vagrant user folder, but the github folder, since this is the folder that github will ssh into and re-deploy in.

Before the vagrant file can be used to provision a new VM, it is necesarry to allow traffic from the new droplet to the database. Otherwise the deploy.sh script is stuck in an endless while loop.

We can add / edit an exisitng firewall by using the digital ocean Command Line Interface (doctl), before we execute the deploy.sh script.


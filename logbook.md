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
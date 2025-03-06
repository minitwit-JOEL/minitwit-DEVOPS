## Week 2

- Main task: Rewrite the application from Python Flask
  
We have chosen to reimplement the application as backend C# API, and a frontend Node.js server, built with the Express framework.

For the backend API we are using the AspNet.Core.Mvc package. We have controllers that contian behavouir for each endpoint, and services, which contian the database interactions, that the controllers call.

The Flask application uses server-side session storage to hold user information between HTTP requests. We have shifted to using JWT-tokens, that is send on login, and which the client sends with every request.

Instead of calling the renderer_template and sending back html, we are sending JSON to the Node.js server, which then sends html to the client.

As a small security hardening, we are no longer telling users from the API explicit, if they are entering a wrong username or password upon login, but only that one of them is wrong.

We have changed the database from Sqlite to using postgress server through Microsoft's EF Core.

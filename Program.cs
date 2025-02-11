using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

// Configuration
string DATABASE = "/tmp/minitwit.db";
int PER_PAGE = 30;
bool DEBUG = true;
string SECRET_KEY = "development key";

SqliteConnection connect_db()
{
    /* Returns a new connection to the database. */

    SqliteConnection connection = new SqliteConnection("Data source=" + DATABASE);
    connection.Open();
    return connection;
}

void init_db()
{
    /* Creates the database tables. */
    SqliteConnection connection = connect_db();

    string? schema = null;

    try
    {
        // Open the text file using a stream reader.
        using StreamReader reader = new("schema.sql");

        // Read the stream as a string.
        schema = reader.ReadToEnd();

    }
    catch (IOException e)
    {
        Console.WriteLine("Could not read from file 'schema.sql. \n Resulted in following error: " + e);
    }

    if (schema is not null)
    {
        using var command = connection.CreateCommand();
        command.CommandText = schema;
        command.ExecuteReader();
    }

    connection.Close();
}

List<Dictionary<string, string>> query_db(string query, SqliteParameter[]? args = null, bool one = false)
{
    /* Queries the database and returns a list of dictionaries. */

    SqliteConnection connection = connect_db();
    var command = connection.CreateCommand();
    command.CommandText = query;

    if (args is not null)
        command.Parameters.AddRange(args);

    List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string column_name = reader.GetName(i);
                string column_value = reader.GetString(i);

                dict.Add(column_name, column_value);
            }

            list.Add(dict);
        }
    }

    connection.Close();

    if (one)
    {
        return list.GetRange(0, 1);
    }
    else
    {
        return list;
    }
}

long? get_user_id(string username)
{
    /* Convenience method to look up the id for a username. */

    SqliteConnection connection = connect_db();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT user_id FROM user WHERE username = @Username";
    command.Parameters.Add(new SqliteParameter("@Username", username));

    long? user_id = null;

    using (var reader = command.ExecuteReader())
    {
        user_id = reader.GetInt64(0);
    }

    connection.Close();

    return user_id;
}

app.MapGet("/public", (HttpContext context) =>
{
    /* Displays the latest messages of all users. */

    string query = @"
        SELECT message.*, user.* 
        FROM message, user 
        WHERE message.author_id = user.user_id 
          AND message.flagged = 0 
        ORDER BY message.pub_date DESC 
        LIMIT @Per_page
    ";

    SqliteParameter[] parameters = [
        new SqliteParameter("@Per_page", PER_PAGE)
    ];

    return Results.Json(query_db(query, parameters));
});

app.MapGet("/{username}", (HttpContext context, string username) =>
{
    listOfUsers = query_db(
        "SELECT * FROM user WHERE username = @Username",
        new[] { newSqliteParameter("@Username", username) },
        one: true);

    if (userList.Count == 0)
        return Results.NotFound($"User '{username} not found.")


    var profileUser = userList[0];

    bool following = false;
    var currentUserId = context.Session.GetString("user_id");

    if (!string.IsNullOrEmpty(currentUserId))
    {
        var followList = query_db(
            "SELECT 1 FROM follower WHERE who_id = @CurrentUserId AND whom_id = @ProfileUserId",
            new[]
            {
                new SqliteParameter("@CurrentUserId", currentUserId),
                new SqliteParameter("@ProfileUserId", profileUser["user_id"])
            },
            one: true);
        following = followList.Count > 0;
    }

    var messages = query_db(@"
        SELECT message.*, user.* 
        FROM message, user 
        WHERE user.user_id = message.author_id 
          AND user.user_id = @ProfileUserId
        ORDER BY message.pub_date DESC 
        LIMIT @PerPage",
         new[]
         {
            new SqliteParameter("@ProfileUserId", profileUser["user_id"]),
            new SqliteParameter("@PerPage", PER_PAGE)
         });


    return Results.Json(new
    {
        profile_user = profileUser,
        followed = followed,
        messages = messages
    });
});

app.MapGet("/{username}/follow", (HttpContext context, string username) =>
{
    var currentUserId = context.Session.GetString("user_id");
    if (string.IsNullOrEmpty(currentUserId))
        return Results.Unauthorized();

    long? otherId = get_user_id(username);
    if (otherId == null)
        return Results.NotFound($"User '{username}' not found ")


     using (var connection = connect_db())
    {
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO follower (who_id, whom_id) VALUES (@CurrentUserId, @WhomId)";
        command.Parameters.Add(new SqliteParameter("@CurrentUserId", currentUserId));
        command.Parameters.Add(new SqliteParameter("@WhomId", whomId));
        command.ExecuteNonQuery();
    }

    // we could return a json msg here? 
    return Results.Redirect($"/{username}");
});

app.MapGet("/{username}/unfollow", (HttpContext context, string username) =>
{
    var currentUserId = context.Session.GetString("user_id");
    if (string.IsNullOrEmpty(currentUserId))
        return Results.Unauthorized();

    long? whomId = get_user_id(username);
    if (whomId == null)
        return Results.NotFound($"User '{username}' not found.");

    using (var connection = connect_db())
    {
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM follower WHERE who_id = @CurrentUserId AND whom_id = @WhomId";
        command.Parameters.Add(new SqliteParameter("@CurrentUserId", currentUserId));
        command.Parameters.Add(new SqliteParameter("@WhomId", whomId));
        command.ExecuteNonQuery();
    }

    // again we could send a msg here? 
    return Results.Redirect($"/{username}");
});




string format_datetime(long timestamp)
{
    /* Format a timestamp for display. */
    DateTimeOffset datetime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
    return datetime.ToString("yyyy-mm-dd @ hh:mm");
}

string gravatar_url(string email, int size = 80)
{
    /* Return the gravatar image for the given email address. */
    UTF8Encoding encode = new UTF8Encoding();
    MD5 md5 = MD5.Create();

    byte[] hash = md5.ComputeHash(encode.GetBytes(email.Trim().ToLower()));
    string hexadec = Convert.ToHexString(hash).ToLower();

    return $"http://www.gravatar.com/avatar/{hexadec}?d=identicon&s={size}";
}

app.MapGet("/", (HttpContext context, HttpRequest request, [FromQuery(Name = "offset")] int? offset) =>
{

    Console.WriteLine("We got a visitor from: " +
    request.HttpContext.Connection.RemoteIpAddress);

    string? user_id = context.Session.GetString("user_id");

    if (string.IsNullOrEmpty(user_id))
        return Results.Redirect("/pulic");

    string query =
    @"
        SELECT message.*, user.* from message, user
        WHERE message.flagged = 0 AND 
        message.author_id = user.user_id AND (
        user.user_id = @User_id OR 
        user.user_id IN (SELECT whom_id FROM follower
        WHERE who_id = @User_id))
        ORDER BY message.pub_date DESC LIMIT @Per_page
    ";

    SqliteParameter[] parameters = [
        new SqliteParameter("@User_id", user_id),
        new SqliteParameter("@Per_page", PER_PAGE)
    ];

    return Results.Json(query_db(query, parameters));
});

app.MapPost("/add_message", async (
    HttpContext context,
    HttpRequest request,
    [FromForm] string text) =>
{
    // Check if user is authenticated
    string? userId = context.Session.GetString("user_id");
    if (string.IsNullOrEmpty(userId))
    {
        return Results.StatusCode(401); // Unauthorized
    }

    if (!string.IsNullOrWhiteSpace(text))
    {
        long pubDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Insert the message into the database
        using (var connection = connect_db())
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO message (author_id, text, pub_date, flagged)
                VALUES (@AuthorId, @Text, @PubDate, 0)";
            command.Parameters.Add(new SqliteParameter("@AuthorId", userId));
            command.Parameters.Add(new SqliteParameter("@Text", text));
            command.Parameters.Add(new SqliteParameter("@PubDate", pubDate));

            await command.ExecuteNonQueryAsync();
        }

        context.Session.SetString("message", "Your message was recorded");
    }

    return Results.Redirect("/timeline");
});


app.MapPost("/login", (
    HttpContext context,
    HttpRequest request,
    [FromForm] string username,
    [FromForm] string password) =>
{

    string? user_id = context.Session.GetString("user_id");

    if (string.IsNullOrEmpty(user_id))
        return Results.Redirect("/pulic");

    string? error = null;

    string query = "SELECT * FROM user WHERE username = @Username";
    SqliteParameter[] parameters = [
        new SqliteParameter("@Username", username),
    ];
    List<Dictionary<string, string>> user = query_db(query, parameters, true);

    if (user[0].Count == 0)
    {
        return Results.Json("Invalid username or password");
    }

    UTF8Encoding utf8 = new UTF8Encoding();
    SHA1 sha1 = SHA1.Create();
    string hash = utf8.GetString(sha1.ComputeHash(utf8.GetBytes(password)));

    if (hash != user[0]["password"])
    {
        return Results.Json("Invalid username or password");
    }

    /* Note: Here the flask application first displays to the user
        that they were logged in and then redirects the user.
        This are possible because it is server-side rendered.
        For now, this is unimplemented in this version of the application. */

    context.Session.SetString("user_id", user[0]["user_id"]);
    return Results.Redirect("/");
});

app.MapPost("/register", async (
    HttpContext context,
    HttpRequest request,
    [FromForm] string username,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string password2) =>
{
    string? user_id = context.Session.GetString("user_id");

    // Redirect if already logged in
    if (!string.IsNullOrEmpty(user_id))
    {
        return Results.Redirect("/timeline");
    }

    string? error = null;

    // Form validation
    if (string.IsNullOrWhiteSpace(username))
    {
        error = "You have to enter a username";
    }
    else if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
    {
        error = "You have to enter a valid email address";
    }
    else if (string.IsNullOrWhiteSpace(password))
    {
        error = "You have to enter a password";
    }
    else if (password != password2)
    {
        error = "The two passwords do not match";
    }
    else if (get_user_id(username) != null)
    {
        error = "The username is already taken";
    }

    if (error != null)
    {
        return Results.Json(new { error });
    }

    // Hash the password using BCrypt
    string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

    // Insert user into the database
    using (var connection = connect_db())
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO user (username, email, pw_hash)
            VALUES (@Username, @Email, @PwHash)";
        command.Parameters.Add(new SqliteParameter("@Username", username));
        command.Parameters.Add(new SqliteParameter("@Email", email));
        command.Parameters.Add(new SqliteParameter("@PwHash", passwordHash));

        await command.ExecuteNonQueryAsync();
    }

    context.Session.SetString("message", "You were successfully registered and can login now");

    return Results.Redirect("/login");
});


app.MapGet("/logout", (HttpContext context) =>
{

    /* Note: Here the flask application first displays to the user
       that they were logged out and then redirects the user.
       This are possible because it is server-side rendered.
       For now, this is unimplemented in this version of the application. */

    context.Session.Remove("user_id");

    return Results.Redirect("/public");
});

app.Run();

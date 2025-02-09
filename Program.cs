using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Configuration
string DATABASE = "/tmp/minitwit.db";
int PER_PAGE = 30;
bool DEBUG = true;
string SECRET_KEY = "development key";

SqliteConnection connect_db() {
    /* Returns a new connection to the database. */

    SqliteConnection connection = new SqliteConnection("Data source=" + DATABASE);
    connection.Open();
    return connection;
}

void init_db() {
    /* Creates the database tables. */
    SqliteConnection connection = connect_db();

    string? schema = null;

    try {
        // Open the text file using a stream reader.
        using StreamReader reader = new("schema.sql");

        // Read the stream as a string.
        schema = reader.ReadToEnd();

    } catch (IOException e) {
        Console.WriteLine("Could not read from file 'schema.sql. \n Resulted in following error: " + e);
    }

    if (schema is not null) {
        using var command = connection.CreateCommand();
        command.CommandText = schema;
        command.ExecuteReader();
    }

    connection.Close();
}

List<Dictionary<string, string>> query_db(string query, SqliteParameter[]? args = null, bool one=false) {
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

    if (one) {
        return list.GetRange(0,1);
    } else {
        return list;
    }
}

long? get_user_id(string username) {
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

string format_datetime(long timestamp) {
    /* Format a timestamp for display. */
    DateTimeOffset datetime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
    return datetime.ToString("yyyy-mm-dd @ hh:mm");
}

string gravatar_url(string email, int size=80) {
    /* Return the gravatar image for the given email address. */
    UTF8Encoding encode = new UTF8Encoding();
    MD5 md5 = MD5.Create();

    byte[] hash = md5.ComputeHash(encode.GetBytes(email.Trim().ToLower()));
    string hexadec = Convert.ToHexString(hash).ToLower();

    return $"http://www.gravatar.com/avatar/{hexadec}?d=identicon&s={size}";
}

app.MapGet("/", (HttpRequest request, [FromQuery(Name = "offset")] int? offset) => {

    Console.WriteLine(offset);

    Console.WriteLine("We got a visitor from: " + 
    request.HttpContext.Connection.RemoteIpAddress);

    // TODO: insert session check and redirect to /public here

    // TODO: retrieve user_id from session
    int user_id = 0;

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

    return query_db(query, parameters);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Run();

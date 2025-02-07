using System.Collections;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// configuration
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

List<Dictionary<string, string>> query_db(string query, SqliteParameter[]? args = null, bool one=false) {
    /* Queries the database and returns a list of dictionaries. */
    SqliteConnection connection = connect_db();
    var command = connection.CreateCommand();
    command.CommandText = query;
    
    if (args is not null)
        command.Parameters.AddRange(args);
    
    List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

    using (var reader = command.ExecuteReader()) {
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

    if (one) {
        return list.GetRange(0,1);
    } else {
        return list;
    }
}

void get_user_id(string username) {
    /* Convenience method to look up the id for a username. */
}

string format_datetime(long timestamp) {
    /* Format a timestamp for display. */
    DateTimeOffset datetime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
    return datetime.ToString("yyyy-mm-dd @ hh:mm");
}

void gravatar_url(string email, int size=80) {
    /* Return the gravatar image for the given email address. */
}

app.MapGet("/", () => {

    SqliteConnection connection = connect_db();

    var command = connection.CreateCommand();
    command.CommandText =
    @"
        SELECT * FROM message
    ";

    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            var name = reader.GetString(0);

            Console.WriteLine($"Hello, {name}!");
        }
    }
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

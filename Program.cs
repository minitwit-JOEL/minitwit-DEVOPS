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
    return new SqliteConnection("Data source=" + DATABASE);
}

void query_db(string query, bool one=false) {
    /* Queries the database and returns a list of dictionaries. */
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
    
    connection.Open();

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

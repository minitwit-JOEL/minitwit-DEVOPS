using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// configuration
string DATABASE = "/tmp/minitwit.db";
int PER_PAGE = 30;
bool DEBUG = true;
string SECRET_KEY = "development key";

app.MapGet("/", () => {

    using (var connection = new SqliteConnection("Data Source=" + DATABASE))
    {
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

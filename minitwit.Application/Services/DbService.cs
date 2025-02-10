using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using minitwit.Application.Interfaces;

namespace minitwit.Application.Services;

public class DbService : IDbService
{
    private readonly string DATABASE = "/tmp/minitwit.db";
    public readonly int PerPage = 30;
    public bool Debug = true;
    private readonly string SecretKey = "development key";
    
    private SqliteConnection connect_db() {
        /* Returns a new connection to the database. */
        SqliteConnection connection = new SqliteConnection("Data source=" + DATABASE);
        connection.Open();
        return connection;
    }
    
    public void init_db()
    {
        using var connection = connect_db();
        
        var schema = string.Empty;

        try {
            // Open the text file using a stream reader.
            using StreamReader reader = new("schema.sql");

            // Read the stream as a string.
            schema = reader.ReadToEnd();

        } catch (IOException e) {
            Console.WriteLine("Could not read from file 'schema.sql. \n Resulted in following error: " + e);
        }

        if (!string.IsNullOrEmpty(schema)) {
            using var command = connection.CreateCommand();
            command.CommandText = schema;
            command.ExecuteReader();
        }

        connection.Close();
    }

    public List<Dictionary<string, string>> query_db(string query, SqliteParameter[]? args = null, bool one = false)
    {
        using var connection = connect_db();
        var command = connection.CreateCommand();
        command.CommandText = query;

        if (args is not null)
        {
            command.Parameters.AddRange(args);
        }
    
        var list = new List<Dictionary<string, string>>();

        using (var reader = command.ExecuteReader()) 
        {
            while (reader.Read())
            {
                var dict = new Dictionary<string, string>();

                for (var i = 0; i < reader.FieldCount; i++) 
                {
                    var columnName = reader.GetName(i);
                    var columnValue = reader.GetString(i);

                    dict.Add(columnName, columnValue);
                }

                list.Add(dict);
            }
        }
        
        connection.Close(); 
        
        if (one && list.Count > 1)
        {
            return [list.First()];
        }
    
        return list;
    }

    public long? get_user_id(string username)
    {
        using var connection = connect_db();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT user_id FROM user WHERE username = @Username";
        command.Parameters.Add(new SqliteParameter("@Username", username));

        long? userId = null;

        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                userId = reader.GetInt64(0);
            }
        }

        connection.Close();

        return userId;
    }

    public string format_datetime(long timestamp)
    {
        var datetime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return datetime.ToString("yyyy-mm-dd @ hh:mm");    }

    public string gravatar_url(string email, int size = 80)
    {
        var encode = new UTF8Encoding();
        var md5 = MD5.Create();

        var hash = md5.ComputeHash(encode.GetBytes(email.Trim().ToLower()));
        var hexadec = Convert.ToHexString(hash).ToLower();

        return $"http://www.gravatar.com/avatar/{hexadec}?d=identicon&s={size}";
    }
}
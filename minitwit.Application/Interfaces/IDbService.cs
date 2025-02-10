using Microsoft.Data.Sqlite;

namespace minitwit.Application.Interfaces;

public interface IDbService
{
    public void init_db();
    List<Dictionary<string, string>> query_db(string query, SqliteParameter[]? args = null, bool one = false);
    long? get_user_id(string username);
    string format_datetime(long timestamp);
    string gravatar_url(string email, int size = 80);
}
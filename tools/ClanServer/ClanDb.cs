using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Npgsql;

sealed class ClanDb : IDisposable
{
    static readonly Regex Parameters = new(@"\$([a-z][a-z0-9_]*)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    public DbConnection Connection { get; }
    public DbTransaction? Transaction { get; set; }

    public ClanDb(DbConnection connection)
    {
        Connection = connection;
        try { Connection.Open(); }
        catch { Connection.Dispose(); throw; }
    }

    public ClanDb(string sqlite) : this(new SqliteConnection(sqlite)) { }

    public DbTransaction Begin(bool readOnly)
    {
        if (Connection is SqliteConnection sqlite) return sqlite.BeginTransaction(readOnly);
        return Connection.BeginTransaction(readOnly ? IsolationLevel.RepeatableRead : IsolationLevel.ReadCommitted);
    }

    public void LockWriter()
    {
        // The SQLite store serializes writers before their first read. PostgreSQL
        // uses the same policy, even when two app instances overlap during deploy.
        if (Connection is NpgsqlConnection) Run("SELECT pg_advisory_xact_lock(1297564743, hashtext(current_schema()))");
    }

    DbCommand Command(string query, (string,object?)[] parameters)
    {
        var command = Connection.CreateCommand();
        command.CommandText = Connection is NpgsqlConnection ? Parameters.Replace(query,"@$1") : query;
        command.Transaction = Transaction;
        foreach (var (key,value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = Connection is NpgsqlConnection ? key.TrimStart('$') : key;
            if (value == null) parameter.DbType = DbType.String;
            parameter.Value = value switch { null => DBNull.Value, bool flag => flag ? 1 : 0, _ => value };
            command.Parameters.Add(parameter);
        }
        return command;
    }

    public int Run(string query, params (string,object?)[] parameters)
    {
        using var command = Command(query,parameters);
        return command.ExecuteNonQuery();
    }

    public T? Value<T>(string query, params (string,object?)[] parameters)
    {
        using var command = Command(query,parameters);
        object? result = command.ExecuteScalar();
        if (typeof(T) == typeof(long) && result is int value) result = (long)value;
        return result is T typed ? typed : default;
    }

    public T[] Read<T>(string query, Func<DbDataReader,T> map, params (string,object?)[] parameters)
    {
        using var command = Command(query,parameters);
        using var reader = command.ExecuteReader();
        var result = new List<T>();
        while (reader.Read()) result.Add(map(reader));
        return result.ToArray();
    }

    public void Dispose() => Connection.Dispose();
}

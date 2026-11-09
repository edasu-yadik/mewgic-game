using System.Text.RegularExpressions;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Npgsql;

sealed partial class ClanStore
{
    readonly object migrationGate = new();
    volatile bool migrated;
    long migrationRetryAt;
    readonly NpgsqlDataSource? postgres;
    readonly string schema = "mewgic";

    public static NpgsqlDataSource CreatePostgres(string connection,string schema,bool development,string? rootCertificate)
    {
        if (!Regex.IsMatch(schema,@"\A[a-z][a-z0-9_]{0,30}\z") || schema is "public" or "information_schema" || schema.StartsWith("pg_",StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid private PostgreSQL schema.");
        var settings = new NpgsqlConnectionStringBuilder(connection)
        {
            SearchPath=schema,
            MaxPoolSize=8,
            Timeout=4,
            CommandTimeout=8,
            IncludeErrorDetail=false
        };
        if (!development && settings.SslMode != SslMode.VerifyFull)
            throw new InvalidOperationException("PostgreSQL requires SSL Mode=VerifyFull.");
        if (!string.IsNullOrWhiteSpace(rootCertificate)) settings.RootCertificate = rootCertificate;
        return NpgsqlDataSource.Create(settings.ConnectionString);
    }

    void EnsurePostgres()
    {
        if (migrated) return;
        lock (migrationGate)
        {
            if (migrated) return;
            if (Environment.TickCount64 < migrationRetryAt)
                throw new ClanError(503,"retry");
            try { MigratePostgres(); }
            catch (DbException error) when (IsDatabaseUnavailable(error))
            {
                migrationRetryAt = Environment.TickCount64+10000;
                throw;
            }
        }
    }

    public static bool IsDatabaseUnavailable(DbException error) =>
        error.IsTransient || error.SqlState is "57P01" or "57P02" or "57P03" or "53300";

    void MigratePostgres()
    {
        using var db = new ClanDb(postgres!.CreateConnection());
        using var tx = db.Begin(false);
        db.Transaction = tx;
        // current_schema() is NULL until the private schema exists.
        db.Run("SELECT pg_advisory_xact_lock(1297564743,hashtext($schema))",("$schema",schema));
        if (db.Value<string>("SELECT schema_name FROM information_schema.schemata WHERE schema_name=$schema",("$schema",schema)) == null)
            db.Run($"CREATE SCHEMA \"{schema}\"");
        db.Run("CREATE TABLE IF NOT EXISTS schema_version(id INTEGER PRIMARY KEY CHECK(id=1), version INTEGER NOT NULL)");
        long version = db.Value<long>("SELECT version FROM schema_version WHERE id=1");
        ApplyMigrations(db,version,index => db.Run("""
            INSERT INTO schema_version VALUES(1,$v) ON CONFLICT(id) DO UPDATE SET version=excluded.version
            """,("$v",index)));
        tx.Commit();
        migrated = true;
    }

    public void CheckDatabase()
    {
        using var db = Open();
        db.Value<int>("SELECT 1");
    }

    public void ImportSqlite(string source)
    {
        if (postgres == null) throw new InvalidOperationException("PostgreSQL is required for import.");
        if (!File.Exists(source)) throw new InvalidOperationException("SQLite backup file does not exist.");
        using var input = new ClanDb(new SqliteConnectionStringBuilder
            { DataSource=Path.GetFullPath(source), Mode=SqliteOpenMode.ReadOnly, ForeignKeys=true, Pooling=false }.ToString());
        using var snapshot = input.Begin(true);
        input.Transaction = snapshot;
        if (input.Value<long>("PRAGMA user_version") != Migrations.Length || input.Value<string>("PRAGMA integrity_check") != "ok" ||
            input.Read("PRAGMA foreign_key_check",r => r.GetString(0)).Length != 0)
            throw new InvalidOperationException("Import requires a consistent, migrated SQLite backup.");

        string[] tables = [
            "players","sessions","session_refresh","clans","members","requests","wins",
            "league_seasons","league_divisions","league_entries","league_event_bonuses","league_rewards",
            "league_wallet_owners","league_clan_emblems","event_clock","event_windows","event_players","event_scores","event_claims"
        ];
        var counts = Write(db =>
        {
            foreach (string table in tables)
                if (db.Value<long>($"SELECT COUNT(*) FROM {table}") != 0)
                    throw new InvalidOperationException("Import requires an empty PostgreSQL destination.");
            var imported = new Dictionary<string,int>();
            foreach (string table in tables)
            {
                using var command = input.Connection.CreateCommand();
                command.Transaction = snapshot;
                command.CommandText = $"SELECT * FROM {table}";
                using var reader = command.ExecuteReader();
                string columns = string.Join(",",Enumerable.Range(0,reader.FieldCount).Select(i => "\""+reader.GetName(i).Replace("\"","\"\"")+"\""));
                string[] parameters = Enumerable.Range(0,reader.FieldCount).Select(i => "$v"+i).ToArray();
                string insert = $"INSERT INTO {table} ({columns}) VALUES ({string.Join(",",parameters)})";
                int count = 0;
                while (reader.Read())
                {
                    db.Run(insert,parameters.Select((key,i) => (key,reader.IsDBNull(i) ? null : reader.GetValue(i))).ToArray());
                    count++;
                }
                if (db.Value<long>($"SELECT COUNT(*) FROM {table}") != count)
                    throw new InvalidOperationException("Import verification failed for "+table);
                imported.Add(table,count);
            }
            return imported;
        });
        foreach (var (table,count) in counts) Console.WriteLine($"{table}: {count}");
    }
}

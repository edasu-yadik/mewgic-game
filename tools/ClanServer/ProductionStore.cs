using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

sealed partial class ClanStore
{
    static readonly string[] Migrations = [
        "001-clans.sql", "002-league.sql", "003-events.sql", "004-sessions.sql"
    ];

    void Migrate() => Write(db =>
    {
        long version = db.Value<long>("PRAGMA user_version");
        if (version < 0 || version > Migrations.Length)
            throw new InvalidOperationException("Unsupported ClanServer database version.");
        for (int index = (int)version; index < Migrations.Length; index++)
        {
            using var resource = typeof(ClanStore).Assembly.GetManifestResourceStream("ClanServer.Migrations."+Migrations[index])
                ?? throw new InvalidOperationException("Missing embedded migration "+Migrations[index]);
            using var reader = new StreamReader(resource);
            db.Run(reader.ReadToEnd());
            db.Run($"PRAGMA user_version={index+1}");
        }
        if (version != Migrations.Length && db.Read("PRAGMA foreign_key_check",r => r.GetString(0)).Length > 0)
            throw new InvalidOperationException("ClanServer database has invalid foreign keys.");
        return true;
    });

    public static void Backup(string source,string destination)
    {
        source = Path.GetFullPath(source);
        destination = Path.GetFullPath(destination);
        if (!File.Exists(source) || File.Exists(destination))
            throw new InvalidOperationException("Backup requires an existing database and a new destination.");
        string temporary = destination+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            using var input = new SqliteConnection(new SqliteConnectionStringBuilder
                { DataSource=source, Mode=SqliteOpenMode.ReadOnly, Pooling=false, DefaultTimeout=30 }.ToString());
            using (var output = new ClanDb(new SqliteConnectionStringBuilder
                { DataSource=temporary, Pooling=false, ForeignKeys=true }.ToString()))
            {
                input.Open();
                input.BackupDatabase(output.Connection);
                if (output.Value<string>("PRAGMA integrity_check") != "ok" ||
                    output.Read("PRAGMA foreign_key_check",r => r.GetString(0)).Length != 0)
                    throw new InvalidOperationException("SQLite backup failed integrity checks.");
            }
            File.Move(temporary,destination);
        }
        finally { File.Delete(temporary); }
    }

    static Session Issue(ClanDb db,string player,long now,long refreshUntil)
    {
        string access = Token(), refresh = Token();
        string accessHash = Hash(access);
        db.Run("DELETE FROM sessions WHERE expires<=$n",("$n",now));
        db.Run("DELETE FROM session_refresh WHERE expires<=$n",("$n",now));
        int lifetime = (int)Math.Min(900,refreshUntil-now);
        db.Run("INSERT INTO sessions VALUES($h,$p,$t)",("$h",accessHash),("$p",player),("$t",now+lifetime));
        db.Run("INSERT INTO session_refresh VALUES($h,$p,$a,$t)",
            ("$h",Hash(refresh)),("$p",player),("$a",accessHash),("$t",refreshUntil));
        return new Session(access,refresh,player,lifetime);
    }

    public Session Refresh(string? refresh) => Write(db =>
    {
        if (!ValidToken(refresh)) throw ClanError.Unauthorized();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string hash = Hash(refresh!);
        var session = db.Read("SELECT player,access_hash,expires FROM session_refresh WHERE hash=$h AND expires>$n",
            r => (Player:r.GetString(0), Access:r.GetString(1), Expires:r.GetInt64(2)),("$h",hash),("$n",now)).SingleOrDefault();
        if (session.Player == null) throw ClanError.Unauthorized();
        db.Run("DELETE FROM session_refresh WHERE hash=$h",("$h",hash));
        db.Run("DELETE FROM sessions WHERE hash=$a",("$a",session.Access));
        return Issue(db,session.Player,now,session.Expires);
    });

    public ActionResult Revoke(string? refresh) => Write(db =>
    {
        if (!ValidToken(refresh)) throw ClanError.Unauthorized();
        string hash = Hash(refresh!);
        db.Run("DELETE FROM sessions WHERE hash=(SELECT access_hash FROM session_refresh WHERE hash=$h)",("$h",hash));
        db.Run("DELETE FROM session_refresh WHERE hash=$h",("$h",hash));
        return new ActionResult("logout");
    });

    static bool ValidToken(string? value) => value?.Length == 64 && value.All(char.IsAsciiHexDigit);

    public AccountProgress VerifiedProgress(string player,int after) => Snapshot(db =>
    {
        if (after < 0 || after > settings.MaxLevel) throw ClanError.Invalid("level");
        var levels = db.Read("SELECT level FROM wins WHERE player=$p AND level>$a ORDER BY level LIMIT 201",
            r => r.GetInt32(0),("$p",player),("$a",after));
        return new AccountProgress(player,levels.Take(200).ToArray(),levels.Length == 201 ? levels[199] : 0);
    });

    public AccountExport ExportAccount(string player) => Snapshot(db =>
    {
        string handle = db.Value<string>("SELECT handle FROM players WHERE id=$p",("$p",player)) ?? throw ClanError.Unauthorized();
        (string,object?)[] args = [("$p",player)];
        var wins = db.Read("SELECT level,day FROM wins WHERE player=$p ORDER BY level",r => new AccountWin(r.GetInt32(0),r.GetString(1)),args);
        var league = db.Read("SELECT season,points,rank FROM league_entries WHERE scope=0 AND entity=$p ORDER BY season",
            r => new AccountLeague(r.GetInt32(0),r.GetInt64(1),r.GetInt32(2)),args);
        var events = db.Read("SELECT event,points FROM event_players WHERE player=$p ORDER BY event",
            r => new AccountEvent(r.GetString(0),r.GetInt64(1)),args);
        var receipts = db.Read("SELECT receipt FROM event_claims WHERE player=$p ORDER BY receipt",r => r.GetString(0),args);
        return new AccountExport(player,handle,Membership(db,player),wins,league,events,receipts);
    });

    public ActionResult DeleteAccount(string player,string? password)
    {
        if (password == null || password.Length is < 10 or > 128) throw ClanError.Forbidden();
        using var lookup = Open();
        var account = lookup.Read("SELECT handle,password FROM players WHERE id=$p",
            r => (Handle:r.GetString(0), Hash:r.GetString(1)),("$p",player)).SingleOrDefault();
        if (account.Handle == null || passwords.VerifyHashedPassword(account.Handle,account.Hash,password) == PasswordVerificationResult.Failed)
            throw ClanError.Forbidden();
        return Write(db =>
        {
            if (db.Value<string>("SELECT password FROM players WHERE id=$p",("$p",player)) != account.Hash)
                throw ClanError.Unauthorized();
            string? clan = Membership(db,player);
            if (clan != null) RemoveMember(db,player,clan,RequireRole(db,player,clan,Member));
            db.Run("""
                DELETE FROM requests WHERE player=$p;
                DELETE FROM session_refresh WHERE player=$p;
                DELETE FROM sessions WHERE player=$p;
                DELETE FROM wins WHERE player=$p;
                DELETE FROM event_scores WHERE entity=$p;
                DELETE FROM event_players WHERE player=$p;
                DELETE FROM event_claims WHERE player=$p;
                DELETE FROM league_entries WHERE scope=0 AND entity=$p;
                DELETE FROM league_divisions WHERE scope=0 AND entity=$p;
                DELETE FROM league_event_bonuses WHERE player=$p;
                DELETE FROM league_rewards WHERE player=$p;
                DELETE FROM league_wallet_owners WHERE player=$p;
                DELETE FROM players WHERE id=$p;
                """,("$p",player));
            return new ActionResult("deleted");
        });
    }
}

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

sealed partial class ClanStore
{
    const int Member = 0, Deputy = 1, Leader = 2;
    readonly string connectionString;
    readonly ClanOptions settings;
    readonly PasswordHasher<string> passwords = new();
    readonly string dummyHash;

    public ClanStore(string path, ClanOptions settings, LeagueOptions league, TimeProvider leagueClock, EventConfig eventConfig)
    {
        this.settings = settings;
        this.league = league;
        this.leagueClock = leagueClock;
        this.eventConfig = eventConfig;
        var file = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        connectionString = new SqliteConnectionStringBuilder { DataSource = file, ForeignKeys = true, DefaultTimeout = 10 }.ToString();
        dummyHash = passwords.HashPassword("", Token());
        using var db = Open();
        db.Run("PRAGMA journal_mode=WAL");
        db.Run("""
            CREATE TABLE IF NOT EXISTS players(id TEXT PRIMARY KEY, handle TEXT NOT NULL UNIQUE, password TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS sessions(hash TEXT PRIMARY KEY, player TEXT NOT NULL REFERENCES players(id), expires INTEGER NOT NULL);
            CREATE INDEX IF NOT EXISTS session_expiry ON sessions(expires);
            CREATE TABLE IF NOT EXISTS clans(id TEXT PRIMARY KEY, name TEXT NOT NULL, normalized TEXT NOT NULL UNIQUE,
              description TEXT NOT NULL, announcement TEXT NOT NULL, emblem INTEGER NOT NULL, open INTEGER NOT NULL, minimum INTEGER NOT NULL);
            CREATE TABLE IF NOT EXISTS members(player TEXT PRIMARY KEY REFERENCES players(id), clan TEXT NOT NULL REFERENCES clans(id),
              role INTEGER NOT NULL CHECK(role BETWEEN 0 AND 2), joined INTEGER NOT NULL);
            CREATE INDEX IF NOT EXISTS clan_roster ON members(clan);
            CREATE UNIQUE INDEX IF NOT EXISTS one_leader ON members(clan) WHERE role=2;
            CREATE TABLE IF NOT EXISTS requests(clan TEXT NOT NULL REFERENCES clans(id) ON DELETE CASCADE,
              player TEXT NOT NULL REFERENCES players(id), invite INTEGER NOT NULL CHECK(invite IN(0,1)), PRIMARY KEY(clan,player,invite));
            CREATE INDEX IF NOT EXISTS requests_by_player ON requests(player);
            CREATE TABLE IF NOT EXISTS wins(player TEXT NOT NULL REFERENCES players(id), level INTEGER NOT NULL,
              clan TEXT REFERENCES clans(id) ON DELETE SET NULL, day TEXT NOT NULL, week TEXT NOT NULL,
              base INTEGER NOT NULL, daily INTEGER NOT NULL, event INTEGER NOT NULL, PRIMARY KEY(player,level));
            CREATE INDEX IF NOT EXISTS clan_wins ON wins(clan,week);
            CREATE INDEX IF NOT EXISTS player_win_day ON wins(player,day);
            """);
        CreateLeagueSchema(db);
        CreateEventSchema(db);
    }

    public Session Register(Credentials input)
    {
        string handle = Handle(input.Handle);
        string password = input.Password ?? "";
        if (password.Length is < 10 or > 128) throw ClanError.Invalid("credentials");
        string hash = passwords.HashPassword(handle, password);
        return Write(db =>
        {
            if (db.Value<long>("SELECT COUNT(*) FROM players WHERE handle=$h", ("$h",handle)) != 0)
                throw ClanError.Conflict("handle_taken");
            string id = Id();
            db.Run("INSERT INTO players VALUES($p,$h,$pw)", ("$p",id), ("$h",handle), ("$pw",hash));
            return Issue(db, id);
        });
    }

    public Session Login(Credentials input)
    {
        string handle = Handle(input.Handle);
        string password = input.Password ?? "";
        if (password.Length is < 10 or > 128) throw ClanError.Unauthorized();
        using var db = Open();
        var player = db.Read("SELECT id,password FROM players WHERE handle=$h", r => (r.GetString(0),r.GetString(1)), ("$h",handle)).SingleOrDefault();
        var checkedHash = passwords.VerifyHashedPassword(handle, player.Item2 ?? dummyHash, password);
        if (player.Item1 == null || checkedHash == PasswordVerificationResult.Failed) throw ClanError.Unauthorized();
        return Write(tx => Issue(tx, player.Item1));
    }

    Session Issue(ClanDb db, string player)
    {
        string token = Token();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        db.Run("DELETE FROM sessions WHERE expires<=$now", ("$now",now));
        db.Run("INSERT INTO sessions VALUES($hash,$p,$expire)", ("$hash",Hash(token)), ("$p",player), ("$expire",now + 86400));
        return new(token);
    }

    public string Authenticate(string token)
    {
        if (token.Length != 64 || !token.All(char.IsAsciiHexDigit)) throw ClanError.Unauthorized();
        using var db = Open();
        return db.Value<string>("SELECT player FROM sessions WHERE hash=$h AND expires>$t", ("$h",Hash(token)),
            ("$t",DateTimeOffset.UtcNow.ToUnixTimeSeconds())) ?? throw ClanError.Unauthorized();
    }

    public ActionResult Logout(string token) => Write(db =>
    {
        db.Run("DELETE FROM sessions WHERE hash=$h", ("$h",Hash(token)));
        return new ActionResult("logout");
    });

    public PlayerState Me(string player) => Snapshot(db =>
    {
        var now = Clock();
        string handle = db.Value<string>("SELECT handle FROM players WHERE id=$p", ("$p",player))!;
        var invitations = Cards(db, now.Week, """
            WHERE c.id IN(SELECT clan FROM requests WHERE player=$p AND invite=1)
            ORDER BY c.name LIMIT 30
            """, ("$p",player));
        return new PlayerState(handle, Membership(db,player) ?? "", (int)Wins(db,player), invitations,
            settings.WeeklyGoal, settings.WinPoints, settings.DailyPoints, settings.WeekendPoints);
    });

    public SearchResult Search(string? query) => Snapshot(db =>
        new SearchResult(Cards(db,Clock().Week,
            "WHERE instr(c.normalized,$q)>0 OR $q='' ORDER BY points DESC,c.normalized LIMIT 30",
            ("$q", Clean(query,0,40).ToUpperInvariant()))));

    public ClanDetail View(string player, string id) => Snapshot(db => View(db, player, id));

    ClanDetail View(ClanDb db, string player, string id)
    {
        var now = Clock();
        var card = Cards(db, now.Week, "WHERE c.id=$c", ("$c",id)).SingleOrDefault() ?? throw ClanError.Missing();
        int role = Role(db,player,id);
        var roster = db.Read("""
            SELECT p.id,p.handle,m.role,COALESCE(SUM(w.base+w.daily+w.event),0)
            FROM members m JOIN players p ON p.id=m.player
            LEFT JOIN wins w ON w.clan=m.clan AND w.player=m.player
            WHERE m.clan=$c GROUP BY p.id ORDER BY m.role DESC,m.joined,p.id
            """, PlayerRow, ("$c",id));
        var requests = role >= Deputy ? db.Read("""
            SELECT p.id,p.handle,0,0 FROM requests r JOIN players p ON r.player=p.id
            WHERE r.clan=$c AND r.invite=0 ORDER BY p.handle LIMIT 50
            """, PlayerRow, ("$c",id)) : [];
        string announcement = db.Value<string>("SELECT announcement FROM clans WHERE id=$c", ("$c",id))!;
        bool requested = HasRequest(db,id,player,false);
        bool invited = HasRequest(db,id,player,true);
        bool weekly = db.Value<long>("SELECT COUNT(*) FROM (SELECT week FROM wins WHERE clan=$c GROUP BY week HAVING SUM(base+daily+event)>=$g)",
            ("$c",id), ("$g",settings.WeeklyGoal)) > 0;
        return new(card,announcement,role,roster,requests,requested,invited,card.Points>0,card.Points>=100,weekly,now.Week,now.Weekend);
    }

    public ClanDetail Create(string player, ClanInput input)
    {
        var validated = Validate(input);
        return Write(db =>
        {
            EnsureFree(db,player);
            EnsureName(db,validated.Name,"");
            string id = Id();
            db.Run("INSERT INTO clans VALUES($c,$n,$u,$d,$a,$e,$o,$w)", ClanParameters(id,validated));
            AddMember(db,id,player,Leader);
            return View(db,player,id);
        });
    }

    public ClanDetail Edit(string player, string id, ClanInput input)
    {
        var validated = Validate(input);
        return Write(db =>
        {
            RequireRole(db,player,id,Leader);
            EnsureName(db,validated.Name,id);
            db.Run("UPDATE clans SET name=$n,normalized=$u,description=$d,announcement=$a,emblem=$e,open=$o,minimum=$w WHERE id=$c",
                ClanParameters(id,validated));
            return View(db,player,id);
        });
    }

    public ActionResult Join(string player, string id) => Write(db =>
    {
        if (Membership(db,player) == id) return new ActionResult("joined");
        EnsureEligible(db,id,player);
        if (db.Value<long>("SELECT open FROM clans WHERE id=$c", ("$c",id)) == 1)
        {
            AddMember(db,id,player,Member);
            return new ActionResult("joined");
        }
        if (HasRequest(db,id,player,false)) return new ActionResult("requested");
        if (db.Value<long>("SELECT COUNT(*) FROM requests WHERE player=$p AND invite=0", ("$p",player)) >= 10 ||
            db.Value<long>("SELECT COUNT(*) FROM requests WHERE clan=$c AND invite=0", ("$c",id)) >= 50)
            throw ClanError.Conflict("request_limit");
        db.Run("INSERT INTO requests VALUES($c,$p,0)", ("$c",id), ("$p",player));
        return new ActionResult("requested");
    });

    public ActionResult Invite(string officer, string id, string? handle) => Write(db =>
    {
        RequireRole(db,officer,id,Deputy);
        string target = db.Value<string>("SELECT id FROM players WHERE handle=$h", ("$h",Handle(handle))) ?? throw ClanError.Missing();
        EnsureEligible(db,id,target);
        if (HasRequest(db,id,target,true)) return new ActionResult("invited");
        if (db.Value<long>("SELECT COUNT(*) FROM requests WHERE player=$p AND invite=1", ("$p",target)) >= 20 ||
            db.Value<long>("SELECT COUNT(*) FROM requests WHERE clan=$c AND invite=1", ("$c",id)) >= 30)
            throw ClanError.Conflict("request_limit");
        db.Run("INSERT INTO requests VALUES($c,$p,1)", ("$c",id), ("$p",target));
        return new ActionResult("invited");
    });

    public ActionResult AnswerRequest(string officer, string id, string target, string action) => Write(db =>
    {
        RequireRole(db,officer,id,Deputy);
        return Answer(db,id,target,false,action);
    });

    public ActionResult AnswerInvite(string player, string id, string action) =>
        Write(db => Answer(db,id,player,true,action));

    ActionResult Answer(ClanDb db, string id, string target, bool invite, string action)
    {
        if (action is not ("accept" or "decline")) throw ClanError.Invalid("action");
        if (!HasRequest(db,id,target,invite)) throw ClanError.Missing();
        if (action == "accept")
        {
            EnsureEligible(db,id,target);
            AddMember(db,id,target,Member);
        }
        else db.Run("DELETE FROM requests WHERE clan=$c AND player=$p AND invite=$i",
            ("$c",id), ("$p",target), ("$i",invite));
        return new(action == "accept" ? "joined" : "declined");
    }

    public ActionResult Leave(string player, string id) => Write(db =>
    {
        int role = RequireRole(db,player,id,Member);
        db.Run("DELETE FROM members WHERE player=$p AND clan=$c", ("$p",player), ("$c",id));
        if (role == Leader)
        {
            string? next = db.Value<string>("SELECT player FROM members WHERE clan=$c ORDER BY role DESC,joined,player LIMIT 1", ("$c",id));
            if (next == null) db.Run("DELETE FROM clans WHERE id=$c", ("$c",id));
            else db.Run("UPDATE members SET role=2 WHERE player=$p", ("$p",next));
        }
        return new ActionResult("left");
    });

    public ActionResult Manage(string officer, string id, string target, string action) => Write(db =>
    {
        int role = RequireRole(db,officer,id,Deputy);
        int current = Role(db,target,id);
        if (current < 0) throw ClanError.Missing();
        if (current >= role) throw ClanError.Forbidden();
        switch (action)
        {
            case "kick":
                db.Run("DELETE FROM members WHERE player=$p AND clan=$c", ("$p",target), ("$c",id));
                break;
            case "promote" when role == Leader && current == Member:
                db.Run("UPDATE members SET role=1 WHERE player=$p AND clan=$c", ("$p",target), ("$c",id));
                break;
            case "demote" when role == Leader && current == Deputy:
                db.Run("UPDATE members SET role=0 WHERE player=$p AND clan=$c", ("$p",target), ("$c",id));
                break;
            default: throw ClanError.Forbidden();
        }
        return new ActionResult(action);
    });

    public bool HasWin(string player, int level)
    {
        using var db = Open();
        return HasWin(db,player,level);
    }

    public WinResult RecordWin(string player, int level, string? expectedClan, VerifiedWin proof) => Write(db =>
    {
        if (HasWin(db,player,level)) return new WinResult(0,true);
        var now = Clock();
        string? clan = Membership(db,player);
        if ((clan ?? "") != (expectedClan ?? "")) throw ClanError.Conflict("membership_changed");
        int daily = 0, weekend = 0, points = clan == null ? 0 : settings.WinPoints;
        if (clan != null)
        {
            if (db.Value<long>("SELECT COUNT(*) FROM wins WHERE player=$p AND day=$d AND daily>0", ("$p",player), ("$d",now.Day)) == 0)
                daily = settings.DailyPoints;
            if (now.Weekend && db.Value<long>("SELECT COUNT(*) FROM wins WHERE player=$p AND week=$w AND event>0", ("$p",player), ("$w",now.Week)) == 0)
                weekend = settings.WeekendPoints;
        }
        db.Run("INSERT INTO wins VALUES($p,$l,$c,$day,$week,$base,$daily,$event)",
            ("$p",player), ("$l",level), ("$c",clan), ("$day",now.Day), ("$week",now.Week),
            ("$base",points), ("$daily",daily), ("$event",weekend));
        AddLeagueWin(db,player,clan,proof);
        AddEventWin(db,player,clan,level,proof);
        return new WinResult(points+daily+weekend,false);
    });

    ClanCard[] Cards(ClanDb db, string week, string tail, params (string,object?)[] args) => db.Read("""
        SELECT c.id,c.name,c.description,c.emblem,c.open,c.minimum,
        (SELECT COUNT(*) FROM members m WHERE m.clan=c.id),
        COALESCE((SELECT SUM(base+daily+event) FROM wins w WHERE w.clan=c.id),0) AS points,
        COALESCE((SELECT SUM(base+daily+event) FROM wins w WHERE w.clan=c.id AND w.week=$week),0),
        COALESCE((SELECT tier FROM league_clan_emblems e WHERE e.clan=c.id),-1)
        FROM clans c
        """ + "\n" + tail, r =>
        {
            long points = r.GetInt64(7);
            return new ClanCard(r.GetString(0),r.GetString(1),r.GetString(2),r.GetInt32(3),r.GetBoolean(4),r.GetInt32(5),
                r.GetInt32(6),settings.Capacity,points,points/settings.PointsPerLevel+1,r.GetInt64(8),settings.WeeklyGoal,r.GetInt32(9));
        }, args.Append(("$week",(object?)week)).ToArray());

    static PlayerCard PlayerRow(SqliteDataReader r) => new(r.GetString(0),r.GetString(1),r.GetInt32(2),r.GetInt64(3));
    static long Wins(ClanDb db, string player) => db.Value<long>("SELECT COUNT(*) FROM wins WHERE player=$p", ("$p",player));
    static bool HasWin(ClanDb db, string player, int level) =>
        db.Value<long>("SELECT COUNT(*) FROM wins WHERE player=$p AND level=$l", ("$p",player), ("$l",level)) > 0;
    static string? Membership(ClanDb db, string player) =>
        db.Value<string>("SELECT clan FROM members WHERE player=$p", ("$p",player));
    static int Role(ClanDb db, string player, string clan) =>
        (int)db.Value<long>("SELECT COALESCE((SELECT role FROM members WHERE player=$p AND clan=$c),-1)", ("$p",player), ("$c",clan));
    static int RequireRole(ClanDb db, string player, string clan, int minimum)
    {
        int role = Role(db,player,clan);
        if (role < minimum) throw ClanError.Forbidden();
        return role;
    }
    static bool HasRequest(ClanDb db, string clan, string player, bool invite) =>
        db.Value<long>("SELECT COUNT(*) FROM requests WHERE clan=$c AND player=$p AND invite=$i",
            ("$c",clan), ("$p",player), ("$i",invite)) > 0;
    static void EnsureFree(ClanDb db, string player)
    {
        if (Membership(db,player) != null) throw ClanError.Conflict("already_member");
    }
    void EnsureEligible(ClanDb db, string id, string player)
    {
        EnsureFree(db,player);
        long minimum = db.Value<long>("SELECT COALESCE((SELECT minimum FROM clans WHERE id=$c),-1)", ("$c",id));
        if (minimum < 0) throw ClanError.Missing();
        if (Wins(db,player) < minimum) throw ClanError.Conflict("requirements");
        if (db.Value<long>("SELECT COUNT(*) FROM members WHERE clan=$c", ("$c",id)) >= settings.Capacity)
            throw ClanError.Conflict("full");
    }
    static void AddMember(ClanDb db, string clan, string player, int role)
    {
        db.Run("INSERT INTO members VALUES($p,$c,$r,$t)", ("$p",player), ("$c",clan), ("$r",role),
            ("$t",DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        db.Run("DELETE FROM requests WHERE player=$p", ("$p",player));
    }
    static void EnsureName(ClanDb db, string name, string clan)
    {
        if (db.Value<long>("SELECT COUNT(*) FROM clans WHERE normalized=$n AND id<>$c",
            ("$n",name.ToUpperInvariant()), ("$c",clan)) > 0) throw ClanError.Conflict("name_taken");
    }

    ClanInput Validate(ClanInput input)
    {
        if (input.Emblem is < 0 or > 2 || input.MinimumWins < 0 || input.MinimumWins > settings.MaxLevel)
            throw ClanError.Invalid("settings");
        return input with { Name=Clean(input.Name,3,26), Description=Clean(input.Description,0,160),
            Announcement=Clean(input.Announcement,0,180) };
    }
    static (string,object?)[] ClanParameters(string id, ClanInput v) =>
        [("$c",id),("$n",v.Name),("$u",v.Name!.ToUpperInvariant()),("$d",v.Description),
         ("$a",v.Announcement),("$e",v.Emblem),("$o",v.Open),("$w",v.MinimumWins)];
    static string Clean(string? input, int minimum, int maximum)
    {
        string text = (input ?? "").Trim().Normalize(NormalizationForm.FormKC);
        if (text.Length < minimum || text.Length > maximum || text.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format))
            throw ClanError.Invalid("text");
        return text;
    }
    static string Handle(string? input)
    {
        string value = (input ?? "").Trim().ToLowerInvariant();
        if (!Regex.IsMatch(value, @"\A[a-z0-9_]{3,20}\z")) throw ClanError.Invalid("credentials");
        return value;
    }
    static string Id() => Guid.NewGuid().ToString("N");
    static string Token() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    static (string Day,string Week,bool Weekend) Clock()
    {
        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        int dayOfWeek = ((int)day.DayOfWeek+6)%7;
        return (day.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture),
            day.AddDays(-dayOfWeek).ToString("yyyy-MM-dd",CultureInfo.InvariantCulture),dayOfWeek>=5);
    }
    ClanDb Open() => new(connectionString);

    T Snapshot<T>(Func<ClanDb,T> action) => Transaction(action,true);
    T Write<T>(Func<ClanDb,T> action) => Transaction(action,false);
    T Transaction<T>(Func<ClanDb,T> action, bool deferred)
    {
        using var db = Open();
        using var tx = db.Connection.BeginTransaction(deferred);
        db.Transaction = tx;
        var value = action(db);
        tx.Commit();
        return value;
    }
}

sealed class ClanDb : IDisposable
{
    public SqliteConnection Connection { get; }
    public SqliteTransaction? Transaction { get; set; }
    public ClanDb(string connectionString)
    {
        Connection = new SqliteConnection(connectionString);
        Connection.Open();
    }
    SqliteCommand Command(string query, (string,object?)[] parameters)
    {
        var command = Connection.CreateCommand();
        command.CommandText = query;
        command.Transaction = Transaction;
        foreach (var (key,value) in parameters) command.Parameters.AddWithValue(key,value ?? DBNull.Value);
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
        return command.ExecuteScalar() is T value ? value : default;
    }
    public T[] Read<T>(string query, Func<SqliteDataReader,T> map, params (string,object?)[] parameters)
    {
        using var command = Command(query,parameters);
        using var reader = command.ExecuteReader();
        var result = new List<T>();
        while (reader.Read()) result.Add(map(reader));
        return result.ToArray();
    }
    public void Dispose() => Connection.Dispose();
}

using System.Net;
using System.Data.Common;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

if (args is ["backup", var source, var destination])
{
    ClanStore.Backup(source,destination);
    return;
}

string? importPath = args is ["import-sqlite", var file] ? file : null;
var builder = WebApplication.CreateBuilder(importPath == null ? args : []);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 24 * 1024);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
var options = builder.Configuration.GetSection("Clans").Get<ClanOptions>() ?? new();
options.Validate();
string? path = builder.Configuration["Database"];
string? pgConnection = builder.Configuration["Postgres:ConnectionString"];
string schema = builder.Configuration["Postgres:Schema"] ?? "mewgic";
bool render = builder.Configuration.GetValue<bool>("Render:EdgeTls");
if (render && !builder.Environment.IsDevelopment() && builder.Configuration["RENDER"] != "true")
    throw new InvalidOperationException("Render edge TLS requires the Render runtime.");
if ((render || importPath != null) && string.IsNullOrWhiteSpace(pgConnection))
    throw new InvalidOperationException("Postgres:ConnectionString is required for Render and SQLite import.");
if (string.IsNullOrWhiteSpace(path) && string.IsNullOrWhiteSpace(pgConnection))
    throw new InvalidOperationException("Configure Database or Postgres:ConnectionString.");
var leagueConfig = builder.Configuration.GetSection("League");
var league = leagueConfig.Get<LeagueOptions>() ?? new();
if (leagueConfig.GetSection("Tiers").Exists())
    league.Tiers = leagueConfig.GetSection("Tiers").Get<LeagueTier[]>()!;
league.Validate();
TimeProvider leagueClock = TimeProvider.System;
if (builder.Configuration["LeagueQaClockFile"] is string qaClock)
{
    string? url = builder.Configuration["urls"];
    if (!builder.Environment.IsDevelopment() || url == null || url.Contains(';') ||
        !Uri.TryCreate(url,UriKind.Absolute,out var address) || address.Host != "127.0.0.1" ||
        builder.Configuration.GetSection("Kestrel:Endpoints").GetChildren().Any())
        throw new InvalidOperationException("League QA clock requires a Development loopback server.");
    leagueClock = new LeagueQaClock(qaClock);
}
var events = EventConfig.Load(builder.Configuration["Events:ConfigPath"] ?? Path.Combine(AppContext.BaseDirectory,"Events.json"),
    builder.Configuration.GetValue<int?>("Events:MaxActive"));
using var postgres = string.IsNullOrWhiteSpace(pgConnection) ? null : ClanStore.CreatePostgres(pgConnection,schema,
    builder.Environment.IsDevelopment(),builder.Configuration["Postgres:RootCertificate"]);
var store = new ClanStore(path,options,league,leagueClock,events,postgres,schema);
if (importPath != null)
{
    store.ImportSqlite(importPath);
    return;
}
builder.Services.AddSingleton(store);
if (!render) builder.Services.AddHostedService<LeagueSeasonWorker>();
builder.Services.AddSingleton(new ClanPuzzles(options.MaxLevel));
builder.Services.AddRateLimiter(rate =>
{
    rate.RejectionStatusCode = 429;
    rate.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 240, Window = TimeSpan.FromMinutes(1) }));
    rate.AddConcurrencyLimiter("proof", o => { o.PermitLimit = 4; o.QueueLimit = 0; });
    rate.AddPolicy("credentials", context =>
        RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();
// ASP.NET Core only trusts its default loopback proxies for forwarded headers.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.Use(async (context, next) =>
{
    // TLS may terminate at a reverse proxy; configure loopback forwarding explicitly for that deployment.
    bool local = context.Connection.RemoteIpAddress is IPAddress address && IPAddress.IsLoopback(address);
    // Render is an explicit edge-termination mode. The platform redirects public
    // HTTP before forwarding HTTP to Kestrel. Client-supplied IP headers remain untrusted.
    if (!render && !context.Request.IsHttps && !local)
    {
        context.Response.StatusCode = 426;
        return;
    }
    context.Response.Headers.CacheControl = "no-store";
    try { await next(); }
    catch (ClanError error)
    {
        if (context.Response.HasStarted) throw;
        context.Response.StatusCode = error.Status;
        if (error.Status == 503) context.Response.Headers.RetryAfter = "10";
        await context.Response.WriteAsJsonAsync(new { error = error.Code });
    }
    catch (DbException error) when (ClanStore.IsDatabaseUnavailable(error))
    {
        if (context.Response.HasStarted) throw;
        context.Response.StatusCode = 503;
        context.Response.Headers.RetryAfter = "10";
        await context.Response.WriteAsJsonAsync(new { error = "retry" });
    }
});
app.UseRateLimiter();
var api = app.MapGroup("/v1");
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).DisableRateLimiting();
app.MapGet("/ready", (ClanStore db) =>
{
    db.CheckDatabase();
    return Results.Ok(new { status = "ready" });
});
app.MapGet("/account/delete", (HttpContext context) =>
{
    context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; script-src 'self'; connect-src 'self'; " +
        "form-action 'none'; base-uri 'none'; frame-ancestors 'none'";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    return Results.File(Path.Combine(AppContext.BaseDirectory,"Account","delete.html"),"text/html; charset=utf-8");
});
app.MapGet("/account/delete.js", () => Results.File(
    Path.Combine(AppContext.BaseDirectory,"Account","delete.js"),"application/javascript; charset=utf-8"));
api.MapPost("/register", (Credentials input, ClanStore store) => store.Register(input)).RequireRateLimiting("credentials");
api.MapPost("/login", (Credentials input, ClanStore store) => store.Login(input)).RequireRateLimiting("credentials");
api.MapPost("/session/refresh", (RefreshInput input, ClanStore store) => store.Refresh(input.RefreshToken)).RequireRateLimiting("credentials");
api.MapPost("/session/revoke", (RefreshInput input, ClanStore store) => store.Revoke(input.RefreshToken)).RequireRateLimiting("credentials");
var clans = api.MapGroup("").AddEndpointFilter(async (call, next) =>
{
    var context = call.HttpContext;
    var auth = context.Request.Headers.Authorization.ToString();
    var store = context.RequestServices.GetRequiredService<ClanStore>();
    if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) throw ClanError.Unauthorized();
    context.Items["player"] = store.Authenticate(auth[7..]);
    return await next(call);
});
clans.MapPost("/logout", (HttpContext c, ClanStore s) => s.Logout(c.Request.Headers.Authorization.ToString()[7..]));
clans.MapGet("/me", (HttpContext c, ClanStore s) => s.Me(Player(c)));
clans.MapGet("/account/progress", (HttpContext c, ClanStore s, int? after) => s.VerifiedProgress(Player(c),after ?? 0));
clans.MapGet("/account/export", (HttpContext c, ClanStore s) => s.ExportAccount(Player(c)));
clans.MapPost("/account/delete", (HttpContext c, ClanStore s, DeleteAccountInput input) => s.DeleteAccount(Player(c),input.Password))
    .RequireRateLimiting("credentials");
clans.MapGet("/clans", (HttpContext c, ClanStore s, string? q) => s.Search(q));
clans.MapPost("/clans", (HttpContext c, ClanStore s, ClanInput input) => s.Create(Player(c), input));
clans.MapGet("/clans/{id}", (HttpContext c, ClanStore s, string id) => s.View(Player(c), id));
clans.MapPut("/clans/{id}", (HttpContext c, ClanStore s, string id, ClanInput input) => s.Edit(Player(c), id, input));
clans.MapPost("/clans/{id}/join", (HttpContext c, ClanStore s, string id) => s.Join(Player(c), id));
clans.MapPost("/clans/{id}/leave", (HttpContext c, ClanStore s, string id) => s.Leave(Player(c), id));
clans.MapPost("/clans/{id}/invite", (HttpContext c, ClanStore s, string id, InviteInput input) => s.Invite(Player(c), id, input.Handle));
clans.MapPost("/clans/{id}/invitations/{action}", (HttpContext c, ClanStore s, string id, string action) =>
    s.AnswerInvite(Player(c), id, action));
clans.MapPost("/clans/{id}/requests/{player}/{action}", (HttpContext c, ClanStore s, string id, string player, string action) =>
    s.AnswerRequest(Player(c), id, player, action));
clans.MapPost("/clans/{id}/members/{player}/{action}", (HttpContext c, ClanStore s, string id, string player, string action) =>
    s.Manage(Player(c), id, player, action));
clans.MapPost("/wins", (HttpContext c, ClanStore s, ClanPuzzles puzzles, WinInput input) =>
{
    string player = Player(c);
    if (input.Level < 1 || input.Level > options.MaxLevel) throw ClanError.Invalid("level");
    if (s.HasWin(player, input.Level)) return new WinResult(0, true);
    var proof = puzzles.Validate(input);
    return s.RecordWin(player, input.Level, input.ClanId, proof);
}).RequireRateLimiting("proof");

api.MapGet("/event-calendar", (ClanStore s, int? page) => s.EventsView(null,page ?? 0));
clans.MapGet("/events", (HttpContext c, ClanStore s, int? page) => s.EventsView(Player(c),page ?? 0));
clans.MapPost("/events/{id}/claim", (HttpContext c, ClanStore s, string id, EventClaimInput input) =>
    s.ClaimEvent(Player(c),id,input));

clans.MapGet("/league", (HttpContext c, ClanStore s, string? scope, int? season) =>
    s.LeagueView(Player(c),scope,season));
clans.MapPost("/league/{season:int}/claim", (HttpContext c, ClanStore s, int season, LeagueClaimInput input) =>
    s.ClaimLeagueReward(Player(c),season,input.WalletId));

app.Run();
static string Player(HttpContext context) => (string)context.Items["player"]!;

using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 24 * 1024);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
var options = builder.Configuration.GetSection("Clans").Get<ClanOptions>() ?? new();
options.Validate();
string path = builder.Configuration["Database"] ?? throw new InvalidOperationException("Database is required.");
builder.Services.AddSingleton(new ClanStore(path, options));
builder.Services.AddSingleton(new ClanPuzzles(options.MaxLevel));
builder.Services.AddRateLimiter(rate =>
{
    rate.RejectionStatusCode = 429;
    rate.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 240, Window = TimeSpan.FromMinutes(1) }));
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
    if (!context.Request.IsHttps && !local)
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
        await context.Response.WriteAsJsonAsync(new { error = error.Code });
    }
});
app.UseRateLimiter();
var api = app.MapGroup("/v1");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
api.MapPost("/register", (Credentials input, ClanStore store) => store.Register(input)).RequireRateLimiting("credentials");
api.MapPost("/login", (Credentials input, ClanStore store) => store.Login(input)).RequireRateLimiting("credentials");
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
    puzzles.Validate(input);
    return s.RecordWin(player, input.Level, input.ClanId);
});

app.Run();
static string Player(HttpContext context) => (string)context.Items["player"]!;

using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

if (args.Contains("--self-test"))
{
    VerificationTest.Run();
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("google", client => client.Timeout = TimeSpan.FromSeconds(5));
builder.Services.AddSingleton<GoogleKeys>();
var app = builder.Build();
var units = (app.Configuration["AdMobUnits"] ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    .Select(id => id[(id.LastIndexOf('/') + 1)..]).ToHashSet(StringComparer.Ordinal);
string root = app.Configuration["ReceiptDirectory"] ?? throw new InvalidOperationException("ReceiptDirectory is required.");
if (units.Count == 0 || units.Any(id => id.Length != 10 || !id.All(char.IsAsciiDigit)))
    throw new InvalidOperationException("AdMobUnits must list the production rewarded unit IDs.");
Directory.CreateDirectory(root);

app.MapGet("/admob/ssv", async (HttpContext context, GoogleKeys keys, CancellationToken cancellation) =>
{
    // Preserve the callback ordering and decode percent escapes only after separating the fields.
    var target = context.Features.Get<IHttpRequestFeature>()?.RawTarget;
    var query = target?.IndexOf('?') is int index && index >= 0 ? target[(index + 1)..] : "";
    var callback = Callback.Parse(query);
    if (callback is null) return Results.BadRequest();
    string? pem = await keys.Find(callback.KeyId, cancellation);
    if (pem is null || !callback.Verify(pem, units, DateTimeOffset.UtcNow)) return Results.BadRequest();
    ReceiptStore.Record(root, callback.Receipt, callback.Transaction);
    return Results.Ok();
});

app.MapGet("/rewarded/{receipt}", (string receipt, HttpContext context) =>
{
    context.Response.Headers.CacheControl = "no-store";
    return Callback.IsReceipt(receipt) && File.Exists(Path.Combine(root, receipt))
        ? Results.Text(receipt)
        : Results.NotFound();
});

app.Run();

sealed record Callback(string Signed, byte[] Signature, string KeyId, string Receipt, string Unit,
    string Transaction, long Timestamp)
{
    public static bool IsReceipt(string value) => value.Length == 32 &&
        Guid.TryParseExact(value, "N", out var id) && id.ToString("N") == value;

    public static Callback? Parse(string raw)
    {
        const string signatureField = "&signature=", keyField = "&key_id=";
        int signatureAt = raw.IndexOf(signatureField, StringComparison.Ordinal);
        int keyAt = raw.IndexOf(keyField, StringComparison.Ordinal);
        if (raw.Length > 4096 || signatureAt < 1 || keyAt <= signatureAt + signatureField.Length) return null;
        string key = raw[(keyAt + keyField.Length)..];
        if (key.Length is < 1 or > 20 || !key.All(char.IsAsciiDigit)) return null;

        string signed = raw[..signatureAt];
        if (signed.Contains("%26", StringComparison.OrdinalIgnoreCase) ||
            signed.Contains("%3D", StringComparison.OrdinalIgnoreCase)) return null;
        var values = QueryHelpers.ParseQuery(signed);
        if (values.Values.Any(value => value.Count != 1)) return null;
        string receipt = values["custom_data"].ToString();
        string unit = values["ad_unit"].ToString();
        string transaction = values["transaction_id"].ToString();
        if (!IsReceipt(receipt) || transaction.Length is < 1 or > 128 || !transaction.All(char.IsAsciiHexDigit) ||
            !long.TryParse(values["timestamp"], NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp))
            return null;
        try
        {
            var signature = WebEncoders.Base64UrlDecode(Uri.UnescapeDataString(raw[(signatureAt + signatureField.Length)..keyAt]).TrimEnd('='));
            return new(signed, signature, key, receipt, unit, transaction, timestamp > 100_000_000_000_000 ? timestamp / 1000 : timestamp);
        }
        catch (FormatException) { return null; }
    }

    public bool Verify(string pem, HashSet<string> units, DateTimeOffset now)
    {
        if (!units.Contains(Unit) || Timestamp < now.AddMinutes(-30).ToUnixTimeMilliseconds() ||
            Timestamp > now.AddMinutes(2).ToUnixTimeMilliseconds()) return false;
        using var key = ECDsa.Create();
        try
        {
            key.ImportFromPem(pem);
            // Match the Google Tink verifier: percent-decode without treating plus as a space.
            return key.VerifyData(Encoding.UTF8.GetBytes(Uri.UnescapeDataString(Signed)), Signature, HashAlgorithmName.SHA256,
                DSASignatureFormat.Rfc3279DerSequence);
        }
        catch (CryptographicException) { return false; }
        catch (ArgumentException) { return false; }
    }
}

sealed class GoogleKeys(IHttpClientFactory clients)
{
    readonly SemaphoreSlim gate = new(1, 1);
    Dictionary<string, string> cache = new(StringComparer.Ordinal);
    DateTimeOffset fetched;

    public async Task<string?> Find(string id, CancellationToken cancellation)
    {
        await gate.WaitAsync(cancellation);
        try
        {
            TimeSpan age = DateTimeOffset.UtcNow - fetched;
            if (age >= TimeSpan.FromHours(12) || age >= TimeSpan.FromMinutes(1) && !cache.ContainsKey(id))
            {
                using var client = clients.CreateClient("google");
                var response = await client.GetFromJsonAsync<KeySet>(
                    "https://www.gstatic.com/admob/reward/verifier-keys.json", cancellation);
                if (response?.Keys is null || response.Keys.Length == 0)
                    throw new InvalidOperationException("Google verification keys unavailable.");
                cache = response.Keys.ToDictionary(key => key.KeyId.ToString(CultureInfo.InvariantCulture), key => key.Pem);
                fetched = DateTimeOffset.UtcNow;
            }
            return cache.GetValueOrDefault(id);
        }
        finally { gate.Release(); }
    }

    sealed record KeySet(GoogleKey[] Keys);
    sealed record GoogleKey(long KeyId, string Pem);
}

static class ReceiptStore
{
    public static bool Record(string directory, string receipt, string transaction)
    {
        // Both receipt and transaction are inside the signature. An exact replay can only name this same receipt.
        string destination = Path.Combine(directory, receipt);
        if (File.Exists(destination)) return false;
        string temporary = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            File.WriteAllText(temporary, transaction);
            File.Move(temporary, destination, overwrite: false);
            return true;
        }
        catch (IOException) when (File.Exists(destination)) { return false; }
        finally { File.Delete(temporary); }
    }
}

static class VerificationTest
{
    public static void Run()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        string pem = key.ExportSubjectPublicKeyInfoPem();
        var now = DateTimeOffset.UtcNow;
        string receipt = Guid.NewGuid().ToString("N");
        const string unit = "1234567890";
        string signed = $"ad_network=1&ad_unit={unit}&custom_data={receipt}&reward_item=Gold%20Coin&timestamp={now.ToUnixTimeMilliseconds()}&transaction_id=abcdef";
        var signature = key.SignData(Encoding.UTF8.GetBytes(Uri.UnescapeDataString(signed)), HashAlgorithmName.SHA256,
            DSASignatureFormat.Rfc3279DerSequence);
        string request = signed + "&signature=" + WebEncoders.Base64UrlEncode(signature) + "&key_id=1";
        var callback = Callback.Parse(request)!;
        Check(callback.Verify(pem, [unit], now), "Valid Google-style percent-decoded signature");
        Check(!Callback.Parse(request.Replace("Gold%20Coin", "Gold+Coin"))!.Verify(pem, [unit], now), "Tampered query");
        Check(!Callback.Parse(request.Replace(receipt, Guid.NewGuid().ToString("N")))!.Verify(pem, [unit], now), "Rebound receipt");
        Check(!callback.Verify(pem, ["9999999999"], now), "Unknown ad unit");
        Check(!callback.Verify(pem, [unit], now.AddHours(1)), "Stale callback");
        Check(Callback.Parse(request + "&custom_data=" + receipt) is null, "Unexpected trailing parameters");
        Check(Callback.Parse(request.Replace("Gold%20Coin", "Gold%26custom_data%3DCoin")) is null,
            "Encoded delimiter cannot alias a signed field");
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var accepted = Enumerable.Range(0, 8).AsParallel()
                .Count(_ => ReceiptStore.Record(dir, receipt, callback.Transaction));
            Check(accepted == 1 && File.ReadAllText(Path.Combine(dir, receipt)) == callback.Transaction,
                "Concurrent duplicate callback persisted exactly once");
        }
        finally
        {
            File.Delete(Path.Combine(dir, receipt));
            Directory.Delete(dir);
        }
    }

    static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        Console.WriteLine("PASS " + name);
    }
}

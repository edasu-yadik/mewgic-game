using System.Text.Json;
using Microsoft.Data.Sqlite;

string dbPath = Path.Combine(Path.GetTempPath(), "mewgic-iap-" + Guid.NewGuid().ToString("N") + ".db");
var config = EventConfig.Load(Path.Combine(AppContext.BaseDirectory,"Events.json"),null);
ClanStore Open() => new(dbPath,new(),new(),TimeProvider.System,config);
void Check(bool passed,string description)
{
    if (!passed) throw new InvalidOperationException(description);
    Console.WriteLine("PASS " + description);
}

try
{
    var server = Open();
    const string password = "qa-isolated-password";
    var owner = server.Register(new("iap_first",password));
    var other = server.Register(new("iap_second",password));
    string player = server.Authenticate(owner.Token);
    var order = new VerifiedStorePurchase("apple","sandbox","123456789","com.mewgic.game.starter",player,2);
    Check(server.GrantVerifiedSandboxPurchase(player,order with { Environment="production" }) == PurchaseGrantResult.Rejected &&
          server.GrantVerifiedSandboxPurchase(player,order with { ProductId="unlisted" }) == PurchaseGrantResult.Rejected &&
          server.GrantVerifiedSandboxPurchase(player,order with { Platform="google",TransactionId="rawToken" }) == PurchaseGrantResult.Rejected &&
          server.GrantVerifiedSandboxPurchase(other.PlayerId,order) == PurchaseGrantResult.Rejected,
        "Canlı ortam, bilinmeyen ürün, açık Google tokenı ve yanlış hesap reddedildi");
    var reopened = Open();
    var result = await Task.WhenAll(Task.Run(() => server.GrantVerifiedSandboxPurchase(player,order)),
        Task.Run(() => reopened.GrantVerifiedSandboxPurchase(player,order)));
    Check(result.Count(value => value == PurchaseGrantResult.Applied) == 1 &&
          result.Count(value => value == PurchaseGrantResult.AlreadyApplied) == 1,
        "İki eşzamanlı sunucu oturumunda tek ödül kaydı");
    using var db = new ClanDb(new SqliteConnectionStringBuilder { DataSource=dbPath, ForeignKeys=true }.ToString());
    Check(db.Read("SELECT gold,extra_bottle,bottle_hammer FROM store_reward_grants",
        row => (row.GetInt64(0),row.GetInt64(1),row.GetInt64(2))).Single() == (2000L,6L,4L),
        "Başlangıç Paketi iki adet için doğru altın ve güçlendiriciler");
    Check(reopened.GrantVerifiedSandboxPurchase(player,order) == PurchaseGrantResult.AlreadyApplied &&
          reopened.GrantVerifiedSandboxPurchase(player,order with { Quantity=1 }) == PurchaseGrantResult.Rejected &&
          reopened.GrantVerifiedSandboxPurchase(other.PlayerId,order with { Owner=other.PlayerId }) == PurchaseGrantResult.Rejected,
        "Yeniden açılışta tekrar tanındı, adet ve hesap değişikliği reddedildi");
    Check(reopened.RecordSandboxStoreConfirmation(order,false) &&
          db.Value<string>("SELECT confirmation_state FROM store_purchases") == "retry" &&
          reopened.RecordSandboxStoreConfirmation(order,true) && !reopened.RecordSandboxStoreConfirmation(order,false) &&
          db.Value<string>("SELECT confirmation_state FROM store_purchases") == "confirmed",
        "Onay hatası ve başarı kalıcı; geç gelen hata tamamlanmış işlemi geri almıyor");

    db.Run("CREATE TRIGGER qa_reject BEFORE INSERT ON store_reward_grants BEGIN SELECT RAISE(ABORT,'qa'); END");
    var failed = order with { TransactionId="987654321" };
    bool rolledBack = false;
    try { reopened.GrantVerifiedSandboxPurchase(player,failed); }
    catch (SqliteException) { rolledBack = db.Value<long>("SELECT COUNT(*) FROM store_purchases") == 1; }
    db.Run("DROP TRIGGER qa_reject");
    Check(rolledBack && reopened.GrantVerifiedSandboxPurchase(player,failed) == PurchaseGrantResult.Applied,
        "Teslim kaydı hatası işlemi geri aldı; tekrar deneme başarılı");

    using var source = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Offers.json")));
    var packs = source.RootElement.GetProperty("gold").EnumerateArray()
        .Concat(source.RootElement.GetProperty("offers").EnumerateArray());
    var columns = new Dictionary<int,int> { [2]=1,[5]=2,[3]=3,[4]=4 };
    int number = 800;
    foreach (var p in packs)
    {
        string sku = p.GetProperty("appleId").GetString()!;
        string tx = (++number).ToString();
        var expected = new long[5];
        int amount = p.TryGetProperty("amount",out var a) ? a.GetInt32() : 0;
        int bonus = p.TryGetProperty("bonus",out var b) ? b.GetInt32() : 0;
        expected[0] = amount + (long)amount*bonus/100;
        if (p.TryGetProperty("powers",out var powers))
            foreach (var power in powers.EnumerateArray())
                expected[columns[power.GetProperty("kind").GetInt32()]] += power.GetProperty("count").GetInt32();
        Check(sku == p.GetProperty("googleId").GetString() &&
              reopened.GrantVerifiedSandboxPurchase(player,order with { ProductId=sku, TransactionId=tx, Quantity=1 }) == PurchaseGrantResult.Applied &&
              db.Read("""
                  SELECT gold,extra_bottle,bottle_hammer,shuffle,color_swap FROM store_reward_grants WHERE transaction_id=$tx
                  """,row => Enumerable.Range(0,5).Select(column => row.GetInt64(column)).ToArray(),("$tx",tx)).Single().SequenceEqual(expected),
            "Sunucu ödülü Unity kataloğuyla eşleşiyor: " + sku);
    }
    Check(number == 807,"Yedi gerçek para SKU'su kontrol edildi");
    reopened.DeleteAccount(player,password);
    Check(db.Value<long>("SELECT COUNT(*) FROM store_reward_grants") == 0 &&
          db.Value<long>("SELECT COUNT(*) FROM store_purchases WHERE player IS NULL") == 9 &&
          reopened.GrantVerifiedSandboxPurchase(other.PlayerId,order with { Owner=other.PlayerId }) == PurchaseGrantResult.Rejected,
        "Hesap silindiğinde ödüller kaldırıldı ve eski işlem yeni hesaba aktarılmadı");
}
finally
{
    SqliteConnection.ClearAllPools();
    foreach (var suffix in new[]{"","-wal","-shm"}) File.Delete(dbPath + suffix);
}

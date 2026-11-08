# MeWGic · Büyülü Klanlar

8 Kasım 2026 · Unity 6000.3.27f2 · `release/0.8.0-rc.1`

Ana menüde **Klan** gerçek sunucuyla çalışan **Büyülü Klanlar** salonunu açar. Oyuncular klan hesabı açıp giriş yapabilir, klan kurabilir, adla arayabilir, açık klana girebilir, onaylı klana istek bırakabilir ve davet kabul edebilir. Salon üye kartlarını, duyuruyu, ortak haftalık hedefi, seviyeyi ve kazanılan başarıları gösterir.

## Mimari ve yetkiler

| Katman | Uygulama |
| --- | --- |
| Unity | `ClanPanel.cs` mevcut mor-altın düğme ve panelleri kullanır. `ClanClient.cs` HTTPS JSON isteklerini ve oturum içi katkı kuyruğunu yönetir. |
| Sunucu | `tools/ClanServer` ASP.NET Core .NET 10 REST servisi. Önceden klan hesabı altyapısı bulunmuyordu. Reklam doğrulama servisi kendi işinde kalıyor. |
| Kayıt | SQLite WAL. Kapasite, katılım, yetki ve puan güncellemeleri yazma işlemi içinde yapılır. Bir hesap aynı anda tek klana üye olabilir. |
| Kimlik | Parola ASP.NET Core PasswordHasher ile tuzlu saklanır. Rastgele oturum anahtarının yalnız özeti sunucuda tutulur. Oturum anahtarı Unity’de bellektedir ve sunucuda 24 saatte sona erer. |
| Puan kanıtı | Sunucu, istemci tarafından bildirilen bir toplam yerine mevcut oyun kurallarıyla aynı bulmacayı oluşturur, döküşleri yeniden oynatır ve tamamlanmayı doğrular. |

**Lider** ayarları ve duyuruyu düzenler, yardımcı atar veya üyeye döndürür. **Yardımcı lider** istek ve davet yönetebilir, sıradan üyeyi çıkarabilir. **Üye** oynayarak katkı yapar. Sunucu her yönetim isteğinde rolü yeniden kontrol eder. Lider ayrıldığında önce en eski yardımcı, yardımcı yoksa en eski üye devralır. Son üyenin ayrılması boş klanı kapatır. Serbest sohbet alanı bulunmuyor.

Puan ve üyelik PlayerPrefs dışında tutulur. Sunucu, istemcinin belirttiği klanı güncel üyelikle karşılaştırır. Eski klana ait bekleyen katkı yeni klana aktarılamaz. Aynı oyuncu ve bölüm için tek kazanış yazılabilir. Bir klana girmeden doğrulanan kazanış katılım şartında sayılır. Bu kazanış daha sonra klana puan getirmez.

## Başlangıç dengesi

| Kural | Varsayılan |
| --- | --- |
| Kapasite | 30 üye |
| Yeni doğrulanmış bölüm | 10 klan puanı |
| Günün ilk klan kazanışı | +5 |
| Cumartesi veya pazar ilk etkinlik katkısı | Haftada bir +15 |
| Haftalık hedef | 150 puan, pazartesi UTC’de yenilenir |
| Klan seviyesi | Her 100 toplam puanda bir seviye |

Sunucu değerleri `Clans:Capacity`, `Clans:WeeklyGoal`, `Clans:WinPoints`, `Clans:DailyPoints`, `Clans:WeekendPoints`, `Clans:PointsPerLevel` ve `Clans:MaxLevel` ayarlarından okunur. Doğrulanabilir bölüm üst sınırı varsayılan olarak 10.000. İpucu ve Geri Al ile oynanan turlar doğrulanabilir. Ek Şişe ve Karıştır kullanılan turlar ile sunucunun yeniden ürettiği düzenden farklı eski arşiv bölümleri klan puanı vermez. Oyun altını ve yerel bölüm kaydı mevcut akışını sürdürür.

## Yerel kullanım ve üretim

Unity açıkken, repo kökünden:

```powershell
dotnet build tools/ClanServer -c Release
python tools/ClanServer/test_http.py
python tools/ClanServer/unity_qa.py
```

Unity QA yardımcısı bir geçici SQLite veritabanı ve gerçek loopback HTTP sunucusu başlatır, **Mewgic → QA → Run Clans** menüsünü çalıştırır ve koşudan sonra sunucuyu kapatır. Elle denemek için:

```powershell
dotnet run --project tools/ClanServer -- --urls http://127.0.0.1:18740 --Database Library/ClanServerDev/clans.db
```

Editor, boş bağlantı ayarında bu loopback adresini kullanır. Mobil üretim ayarı `Assets/Resources/Clans/Connection.json` içindeki `baseUrl` alanıdır ve HTTPS kabul eder. Şu an boş. Üretim için servis, kalıcı SQLite dizini olan bir sunucuya kurulup TLS uç noktası verilmeli. HTTP’de düz metin bağlantı yalnız loopback’e açık. TLS ters vekilde sonlanıyorsa ASP.NET Core’un güvendiği vekil ve başlıklar kullanılmalı. Tek SQLite veritabanıyla tek hizmet örneği çalıştırılır.

Bağlantı kesilince yerel oyun devam eder. Mevcut klan görünümü son sunucu verisini ekranda tutar ve hata bildirir. Bekleyen katkılar oturum belleğinde saklanır, salon yenilendiğinde tekrar denenir. Uygulamanın kapanması bu bekleyen kuyruğu temizler. Hesaba tekrar giriş için oyuncu adı ve parola gerekir. Parola kurtarma ve otomatik bot tespiti bu sürümün kapsamı dışında.

## Kanıt ve görsel onay

29 bağımsız HTTP kontrolü, 165 Unity Play Mode kontrolü ve 975 premium düğme regresyonu geçti. Unity gerçek Kestrel sunucusuyla ve iki ayrı oyuncu oturumuyla çalıştı. 1, 2 ve prosedürel 3. bölümden toplam 50/150 haftalık puan okundu. Birinci bölümün tekrarı puanı artırmadı. HTTP testinde servis gerçekten durduruldu ve aynı SQLite dosyasıyla yeniden açıldı. Unity, ulaşılamayan bir sokete yönlendirilerek ağ hatası ve geri dönüş UI’sı doğrulandı. Unity Console temiz ve yerel oyuncu kaydı korundu. .NET Release derlemesi ve publish başarılı.

[HTTP kontrolleri](HTTP_TESTS.txt) · [Unity Play Mode](PLAY_MODE.json) · [Düğme regresyonu](BUTTONS_REGRESSION.json)

[Hesap girişi](01-Login-9x16.png) · [Klan kurma ve amblemler](02-Create-9x16.png) · [Klan arama](03-Search-9x16.png) · [9:19.5 salon](04-Hall-9x19_5.png) · [9:20 salon](05-Hall-9x20.png) · [Çevrimdışı](06-Offline-9x20.png)

Altın, Kristal ve Pembe amblemler mevcut maskot ve çerçeve varlıklarından hazırlanan seçilebilir önizlemelerdir. Yeni nihai amblem çizimi için görsel onay bekleniyor. Klan ekranı Türkçe ve İngilizce hazır. Diğer altı oyun dili klan alanında İngilizce metni gösteriyor.

Üretim sunucusu ve fiziksel cihaz doğrulaması bekliyor. Testlerde kullanılan hesaplar ve kayıtlar geçici yerel veritabanında oluşturuldu.

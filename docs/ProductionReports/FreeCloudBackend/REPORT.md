# MeWGic · Ücretsiz bulut backend hazırlığı

9 Kasım 2026 · Unity 6000.3.27f2 · PostgreSQL 17.11 · `release/0.8.0-rc.1`

**Durum: yerel hazırlık doğrulandı.** Render ve Supabase üzerinde kaynak açılmadı. HTTPS uç noktası, Docker konteyneri ve gerçek cihaz kabulü bulut hesabı bağlandıktan sonra doğrulanacak.

## Hazırlıkta tamamlananlar

- Mevcut .NET 10 API hem SQLite hem PostgreSQL ile çalışıyor. Paylaşılan SQL geçişleri ve API sözleşmeleri korundu. SQL yazıcı kilidi, iki API işlemi aynı anda çalışırken üyelik sınırı, puan yazımı ve ödül tekilliğini korur. PostgreSQL'de özel `mewgic` şeması, sınırlı bağlantı havuzu ve üretimde sertifika/ana bilgisayar doğrulaması kullanılır.
- `import-sqlite` aracı 19 tabloyu boş PostgreSQL hedefine tek işlemle aktarır. Salt okunur kaynak kopyada bütünlük, yabancı anahtar ve hedef satır sayıları denetlenir. Hesap kimlikleri, parola karmaları, oturumlar, üyelik, puanlar ve ödül makbuzları taşınır. Kısmi hata geri alınır. Gerçek oyuncu verisinin buluta aktarımı hesap bağlantısından sonra yapılır.
- Render için .NET 10 Dockerfile, küçük derleme bağlamı ve yalnız **Free** web servisi içeren Blueprint eklendi. Otomatik deploy kapalı. PostgreSQL parolası Render'ın gizli ortam değişkeninde saklanır. `/health` API sürecini, `/ready` veritabanını denetler. Ulaşılamayan veritabanı 503 ve yeniden deneme yanıtı verir. Render profilinde zamanlanmış arka plan sorguları yerine takvim ve sezon işlemleri ilgili oyuncu isteklerinde ilerler.
- Unity geliştirme ve üretim için ayrı HTTPS kök URL'leri kullanır. 502/503/504, zaman aşımı ve HTML açılış sayfası yeniden denenebilir hata olarak gösterilir. Klan, lig ve etkinlik ekranları son okunan görünümü korur. Başarısı belirsiz yazma istekleri ağ katmanında otomatik tekrarlanmaz. Kazanış kuyruğu mevcut güvenli tekrar mekanizmasını kullanır.

## Yerel kanıt

| Kontrol | Sonuç |
| --- | --- |
| Gerçek PostgreSQL 17.11 API | 101 kontrol geçti. SQLite verisi, hatalı aktarımda rollback, iki API sürecinde eşzamanlı yazma, tek ödül, geçersiz kanıt, API yeniden başlatma ve veritabanı kesintisi doğrulandı. |
| SQLite regresyonları | 117 kontrol geçti. Klan 29, lig 23, etkinlik 37, hesap ve yedek 28. |
| Unity Play Mode → yerel PostgreSQL | 363 kontrol geçti. Klan ve hesap 192, lig 83, etkinlik 88. HTML açılış sayfası ve 503 oturumu korudu. |
| Derleme | C# derlemesi ve Linux x64 sunucu publish geçti. Render YAML'da Free plan ve elle dağıtım denetlendi. Yerel Docker motoru bulunmadığı için konteyner çalıştırılmadı. |

[PostgreSQL sonuçları](POSTGRES.txt) · [SQLite hesap ve yedek](SQLite-test_production.txt) · [SQLite klan](SQLite-test_http.txt) · [SQLite lig](SQLite-test_league.txt) · [SQLite etkinlik](SQLite-test_events.txt) · [Unity klan](Clans/PLAY_MODE.json) · [Unity lig](League/PLAY_MODE.json) · [Unity etkinlik](Events/PLAY_MODE.json)

[Hesap yönetimi Game View](Clans/07-Account-9x20.png) · [Lig](League/04-Players-9x19_5.png) · [Etkinlik merkezi](Events/01-Active-9x16.png)

## Hesap bağlantıları ve gerçek çevrimiçi test

1. Supabase Free projesinde özel veritabanı rolü ve şema oluştur. **Session pooler** bağlantısını 5432 ve `SSL Mode=VerifyFull` ile kullan. Gerekirse eski SQLite kopyasını Render servisini açmadan önce aktar.
2. Render'da GitHub uygulamasına yalnız MeWGic deposu erişimi ver. Blueprint önizlemesinde tek Free Docker Web Service ve otomatik dağıtımın kapalı olduğunu doğrula. `Postgres__ConnectionString` değerini Render'da gir. Gerekirse CA'yı Secret File olarak ekle.
3. Elle ilk deploy'u başlat. Canlı `/health`, `/ready`, HTTPS ve takvim yanıtlarını kontrol et. Unity'de önce geliştirme URL'sini kullan. İki gerçek hesap, ödül tekrarı, Render soğuk başlangıcı, Supabase duraklatma/yeniden açma, API restart ve dış PostgreSQL yedeği geri yüklemesini kabul testinde dene. Üretim URL'sini bu doğrulamadan sonra ayır.

Ücretsiz katmanda Render'ın soğuk başlangıcı ve Supabase'in düşük etkinlikte duraklatılması beklenen davranışlardır. Bu iki bulut davranışı yerelde hata yanıtlarıyla benzetildi. Kota ve çalışma süreleri iki sağlayıcının panosundan izlenir. [Render Free sınırları](https://render.com/docs/free) · [Supabase duraklatma](https://supabase.com/docs/guides/platform/free-project-pausing) · [Adım adım kurulum](../../../tools/ClanServer/Deploy/RENDER_FREE.md)

Ana değişiklikler `ClanDb.cs`, `PostgresStore.cs`, `Program.cs`, `ClanStore.cs`, `EventStore.cs`, `LeagueStore.cs`, `ProductionStore.cs`, `Migrations/`, `Dockerfile`, `.dockerignore` ve `render.yaml`. Unity'de `ClanClient.cs`, `Connection.json`, lig ve etkinlik hata eşlemeleri, klan probu ve yerel test koşucusu güncellendi.

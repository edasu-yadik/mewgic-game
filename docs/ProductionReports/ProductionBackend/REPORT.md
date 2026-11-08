# MeWGic · Üretim backend entegrasyonu

8 Kasım 2026 · Unity 6000.3.27f2 · `release/0.8.0-rc.1`

**Durum: repo hazırlığı ve yerel doğrulama.** Canlı uç nokta açılmadı. Barındırma, DNS ve yedek depolama hesaplarına bağlı bir üretim uç noktası bulunmadı. `Connection.json` üretim adresi boş. Ücretli kaynak açılmadı ve mobil paket alınmadı.

## Mimari ve aylık maliyet

Mevcut **ASP.NET Core / .NET 10 Minimal API + SQLite WAL** korunuyor. Unity aynı HTTPS JSON API'sinden klan, lig, etkinlik ve ödül durumunu okur. Başlangıçta tek Linux API yazıcısı ve Caddy TLS proxy'si kullanılır. Tek yazıcı, mevcut SQLite işlemlerini ve ödül makbuzu kurallarını korur. Klan ve lig açılış ve yenilemede, Etkinlikler görünürken yaklaşık 30 saniyede sunucuyu okur. Daha sonra ölçülen yazma kilidi ve yük gerekçesi oluşursa PostgreSQL'e geçiş değerlendirilir. Bu geçiş hazır bir SQLite dosyasını PostgreSQL'e kopyalamaktan ibaret değildir.

| Seçenek | Aylık tahmin | Değerlendirme |
| --- | --- | --- |
| Mevcut API + Hetzner CX23, Almanya/Finlandiya | €5,49 sunucu + €0,50 IPv4 = **€5,99**, KDV hariç | İlk kontrollü yayına önerilen seçenek. Caddy, tek yazıcı ve işlem kuralları korunur. |
| Aynı VPS + isteğe bağlı sağlayıcı snapshot yedekleri | Yaklaşık **€7,09**, KDV hariç | Sağlayıcı yedeği sunucunun %20'si. Ayrı şifreli SQLite kopyası yine tutulur. |
| Backblaze B2 ile ayrı şifreli yedek | İlk 10 GB içinde depolama $0, üstünde liste fiyatı $6,95/TB/30 gün | Kullanım ve kurtarma trafiği ayrıca izlenir. Alan adı, vergi ve limit aşımı tahmine dahil değildir. |
| Supabase Pro + mevcut API | **$25/ay** plan tabanı, API barındırması ve kullanım eklenir | Yönetilen Postgres ve kimlik doğrulama avantajlı. SQL uyarlaması, yetki modeli ve mevcut hesap göçü gerekir. |
| Firebase Auth ve Firestore | Kullanıma göre, sabit toplam için trafik tahmini gerekir | Kimlik ve mobil SDK'lar uygun. Mevcut ilişkisel işlemler ve sıralama modeli yeni veri erişimi ister. |

Fiyatlar yayımlanmış tarife üzerinden tahmindir. Tek VPS ve mevcut alan adı varsayımında başlangıç bütçesi yaklaşık **€6–8/ay**, KDV ve alan adı hariç. İkinci sürekli sunucu, bağımsız hazırlık ortamı ve operasyon maliyeti dahil değildir. [Hetzner tarife](https://docs.hetzner.com/general/infrastructure-and-availability/price-adjustment/) · [IPv4](https://docs.hetzner.com/general/infrastructure-and-availability/ipv4-pricing/) · [Yedek](https://docs.hetzner.com/cloud/billing/faq/) · [Backblaze](https://www.backblaze.com/cloud-storage/pricing) · [Supabase](https://supabase.com/pricing) · [Firebase](https://firebase.google.com/pricing)

## Hesap, kayıt ve güvenlik

- Benzersiz hesap kimliği cihazlar arasında aynı kalır. Mevcut parola karması korunur. Erişim anahtarı en çok 15 dakika, yenileme ailesi en çok 30 gün geçerlidir. Yenileme anahtarı tek kullanımlıktır, önceki erişim iptal edilir. Sunucu yalnız karmaları saklar. Unity anahtarları bellekte tutar, oturum açıkken otomatik yeniler. Uygulama tamamen kapatılınca yeniden giriş gerekir. Çevrimiçi çıkış her iki anahtarı iptal eder. Ağ kesintisinde cihazdaki oturum kapanır, sunucu iptali isteğin ulaşmasına bağlıdır.
- Oyuncu kendi doğrulanmış bölüm kimliklerini `/v1/account/progress` üzerinden sayfalı okuyabilir. Klan, lig, etkinlik ve kayıtlı ödül durumu aynı hesaba bağlıdır. Cihazdaki altın, kozmetik, yıldız ve kayıt günlüğü yerelde kalır. Eski kayıt otomatik olarak rekabet puanına dönüştürülmez. Geçişte mevcut kayıt korunur, oyuncu bir çevrimiçi hesap açar veya hesabına girer, yeni geçerli hamleler sunucuya gönderilir. Tam çapraz cihaz oyun kaydı için ayrıca kullanıcı onayıyla bağlı ve sürümlü bulut kayıt modeli gerekir. Eski ve yeni bakiye toplanmaz. Ödülün hangi cihaz cüzdanına yazıldığı korunur.
- Bekleyen çevrimiçi kazanışlar hesap ve API adresine göre ayrılmış yerel kuyruğa yazılır. Yeniden girişte veya çevrimiçi ekran yenilenirken yeniden denenir. Sunucu hamleleri tekrar doğrular. Bölüm, etkinlik ve lig puanı sunucu kabul anına göre hesaplanır. Üyelik değişmişse eski klana puan yazılmaz. Giriş bilgileri bu kuyruğa kaydedilmez.
- Unity'de **Klanlar → Hesap** bölümünde veri kopyalama ve parolayla kalıcı silme var. Silme üyeliği, kişisel çevrimiçi puanları ve oturumları kaldırır. Gerekirse liderlik aynı işlemde devredilir. Ortak klan istatistikleri ve telefondaki oyun kaydı kendi kapsamlarında kalır. Web silme yolu `/account/delete` hazırlanmıştır. Mağaza kaydında gerçek URL'yi yayınlama ve gizlilik metinleri açık. [Apple hesap silme](https://developer.apple.com/support/offering-account-deletion-in-your-app/) · [Google Play kullanıcı verisi](https://support.google.com/googleplay/android-developer/answer/10144311)
- HTTPS zorunluluğu, IP hız limiti, bulmaca doğrulama eşzamanlılık sınırı, sunucuda rol denetimi ve tek işlemli ödül kayıtları uygulanır. SQLite doğrudan istemciye açılmaz. Doğrulanmamış istemci puanları kullanılmaz. Oturum kurtarma/parola sıfırlama, cihazda kalıcı güvenli giriş, üretim izleme ve genel kötüye kullanım denetimi açık.
- Sürümlü SQL geçişleri klan, lig, etkinlik ve oturum tablolarını ayırır. Çevrimiçi SQLite yedeği bütünlük ve foreign key kontrolüyle üretilir. Testte ayrı API süreci yedekten açıldı. Şifreli dış depo, zamanlayıcı, silme kaydının kurtarmada tekrar uygulanması ve gerçek kurtarma tatbikatı kurulumu bekliyor. [Dağıtım ve kurtarma kılavuzu](../../../tools/ClanServer/Deploy/README.md)

## Kanıtlar ve değişiklikler

- **117 HTTP/SQLite kontrolü** geçti. 28 yeni üretim hazırlığı kontrolüne klan 29, lig 23 ve etkinlik 37 regresyonu eşlik etti. İki bağımsız HTTP istemcisi aynı klan ve lig durumunu okudu. İkinci cihaz oturumu aynı ödül makbuzunu aldı, başka cüzdana ikinci teslim reddedildi. Geçersiz erişim, eşzamanlı anahtar yenileme, oturum iptali, parola ile silme, sürümsüz veritabanı geçişi ve ayrı süreçte yedekten açılış doğrulandı. Proxy üzerinden açık HTTP isteği reddedildi. Canlı TLS sertifika ve fiziksel cihaz kabulü açık. [Üretim API](test_production.txt) · [Klan API](test_http.txt) · [Lig API](test_league.txt) · [Etkinlik API](test_events.txt)
- **359 Unity Play Mode kontrolü** geçti. Klan 188, lig 83, etkinlik 88. Gerçek yerel sunucuya bağlanan klan probu, süresi dolan erişimi yeniledi. Kesintide yazılan bölüm kanıtı aynı hesapla tekrar girişte diskten okundu ve yalnız bir sunucu kazanışı üretti. Hesap verisi kopyalama, silme ve yerel oyuncu kaydının değişmemesi doğrulandı. [Klan Play Mode](Clans/PLAY_MODE.json) · [Lig Play Mode](League/PLAY_MODE.json) · [Etkinlik Play Mode](Events/PLAY_MODE.json)
- C# derlemesi, sunucu Release derlemesi ve sunucu publish tamamlandı. Üretim kılavuzundaki shell komutu sözdizimi ve web silme betiği kontrol edildi. Caddy, Linux systemd, DNS/TLS ve uzak restic iş akışı henüz barındırma ortamında çalıştırılmadı.
- [Hesap yönetimi Game View](Clans/07-Account-9x20.png) · [Klan salonu](Clans/04-Hall-9x19_5.png) · [Lig sıralaması](League/04-Players-9x19_5.png) · [Etkinlik merkezi](Events/01-Active-9x16.png)

- Sunucu ve şema `ProductionStore.cs`, `Program.cs`, `ClanStore.cs`, `ClanModels.cs`, `LeagueStore.cs`, `EventStore.cs`, `ClanServer.csproj` ve `Migrations/001–004`. Web silme formu `Account/`, dağıtım ve yedek şablonları `Deploy/`.
- Unity bağlantısı `ClanClient.cs`, `ClanClientSessions.cs`, `EventClient.cs`, `ClanPanel.cs`, yerelleştirme tabloları ve `ClanProbe.cs`. Oturum, veritabanı, kurtarma ve iki istemci denetimleri `test_production.py`.
- Üretim için gerekli erişimler onaylı VPS projesi, DNS düzenleme erişimi, ayrı hazırlık alan adı, yedek deposuna kısıtlı yazma kimliği ve şifreleme parolası, sunucu işletim yetkisi, mağaza hesaplarında gizlilik/silme bağlantısı ayarı. Parolalar sohbet veya repo üzerinden paylaşılmaz.
- Kalan sıra maliyet ve sağlayıcı onayı, hazırlık kurulumu, TLS/izleme/yedek denemesi, hesap kurtarma ve silme sonrası yedeklerin yönetimi, iki fiziksel cihazla kabul, ardından üretim alan adı ve istemci yapılandırması.

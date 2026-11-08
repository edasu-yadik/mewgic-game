# MeWGic API · Hazırlık ve üretim kurulumu

Kurulum şablonu bir Linux sunucusunda Caddy ve ASP.NET Core .NET 10 çalışma zamanı varsayar. Üretim ve hazırlık ayrı ana bilgisayar, SQLite dizini, API alan adı ve yedek erişimi kullanır. Her SQLite dosyasının tek API yazıcısı olur. Aynı SQLite dosyasını birden fazla sunucuya bağlamayın.

## Kurulum sırası

1. Onaylanan VPS ve DNS hesabında hazırlık alan adını ayır. Caddy için 80/443, yönetim için kısıtlı SSH aç. 18740 dışarıdan erişilemez kalır. Otomatik güvenlik güncellemelerini ve saat eşitlemesini doğrula.
2. Depodan `dotnet publish tools/ClanServer -c Release -o <çıktı>` ile sunucu dosyalarını hazırla. Sürümü `/opt/mewgic/releases/<commit>` altına yükle. `/opt/mewgic/current` bağlantısını onaylanan sürüme taşı. Salt okunur servis dosyaları ve ayrı `mewgic` işletim sistemi kullanıcısı kullan.
3. `Events.json` dosyasını `/etc/mewgic/Events.json` altına yerleştir. Hizmet kullanıcısına okuma yetkisi ver. `mewgic-api.service` yapılandırmasını yükle. `MEWGIC_API_HOST` değerini Caddy hizmetine ver. `Caddyfile` dosyasını doğrula ve proxy'yi başlat. Örnek dosyalar servis başlatmaz.
4. `https://<alan-adı>/health` ve `https://<alan-adı>/account/delete` adreslerini güvenilir cihazdan kontrol et. TLS zinciri, düz HTTP yönlendirmesi, dışarıdan 18740 erişiminin reddi ve sahte `X-Forwarded-*` başlıklarına karşı davranışı test et. CDN eklenirse güvenilen proxy zincirini ayrıca tasarla. [Caddy proxy başlıkları](https://caddyserver.com/docs/caddyfile/directives/reverse_proxy)
5. Yedekleme, düşük disk alanı, hata oranı, TLS sertifika yenilemesi ve veritabanı kilit beklemesi için ölçüm ve uyarı kur. Hazırlıkta iki ayrı mobil hesapla çalış. Sonuçlar kabul edilince `Assets/Resources/Clans/Connection.json` içine yalnız HTTPS API kökünü yaz. İstemciye anahtar, parolalar veya yedekleme kimliği eklenmez.
6. Her deploy öncesinde kurtarma kopyası al. Geçişler açılışta işlem içinde uygulanır. Daha yeni şema sürümüyle eski uygulama açılmaz. Geri dönüşte önce dağıtımı durdur, ön geçiş kopyasını ayrı dizinde doğrula ve kurtarma ile veri kaybı etkisini değerlendir. Çalışan veritabanını veya WAL dosyasını yerinde değiştirme.
7. Yeni etkinlik ve takvim ayarını önce hazırlıkta doğrula, yapılandırmayı sürümle ve hizmeti kontrollü yeniden başlat. Aktif etkinliğin kural ve ödül kopyası veritabanında kalır. Mevcut kural türleriyle tanımlanan etkinlikler için istemci güncellemesi gerekmez.

## Yedek ve geri yükleme

- Başlangıç hedefi saatte bir çevrimiçi SQLite kopyası, 24 saatlik saatlik geçmiş ve 14 günlük günlük geçmiş. Hedef RPO en çok bir saat, hedef RTO iki saat. Bunlar hazırlıkta tatbikatla ölçülür.
- `dotnet ClanServer.dll backup <kaynak.db> <yeni-kopya.db>` SQLite Online Backup API'sini kullanır. Hedef dosya yeni olmalıdır. Komut yayınlamadan önce bütünlük ve foreign key kontrolü yapar. `backup.sh` izinleri kısıtlı bir geçici kopya oluşturur ve bunu restic ile şifreli depoya gönderir. Kimlik bilgileri ve parola dosyası sunucuya ayrı verilir. Depoya veya Unity istemcisine eklenmez.
- Zamanlayıcıyı ayrı kilitle çalıştır ve başarısızlıkta alarm üret. Saklama kuralı planı `restic forget --tag mewgic-db --keep-hourly 24 --keep-daily 14 --prune`. Silme yetkisi kısıtlı bakım kimliğine verilir. Düzenli `restic check` ve ayrı sunucuda geri yükleme tatbikatı yapılır.
- Geri yüklenen kopya önce kapalı ağda açılır. `PRAGMA integrity_check` ve `PRAGMA foreign_key_check` çalıştırılır. Oturum tablolarındaki erişim ve yenileme kayıtları iptal edilir. Son yedekten sonra işlenen hesap silmeleri bağımsız silme kaydıyla yeniden uygulanır. Bu kaydın dış depoda saklanması ve kurtarma sırasında uygulanması işletime alma şartıdır. Yedekleri saklama süresi sonunda sil.
- Oyuncu kayıtlarını tutan cihaz, klan sunucusu SQLite yedeğine dahil değildir. Üretim yedeğinin şifreli disk, erişim günlüğü ve gizli anahtarların kayıp durumunda kurtarma yolu ayrıca doğrulanır.

# MeWGic · Son entegrasyon ve yayın denetimi

8 Kasım 2026 · Unity 6000.3.27f2 · `release/0.8.0-rc.1` · İncelenen uygulama tabanı `096f561`

**Yayın kararı: hazır değil.** Editör ve yerel servis doğrulaması, üretim bağlantıları ve fiziksel cihaz kabulünden ayrı değerlendirildi. Bu denetimde yeni iOS veya Android paketi oluşturulmadı.

## Sistem durumu

| # | Sistem | Durum | Doğrulama |
| --- | --- | --- | --- |
| 1 | Bölüm ilerlemesi ve üretim | Tamamlandı ve Unity'de doğrulandı | 1→2→3→4→5, 9→10→11, 99→100→101, yeniden açılış ve arşivsiz üretim. |
| 2 | Sonuç ekranı ve yıldızlar | Tamamlandı ve Unity'de doğrulandı | 1–3 yıldız, sıralı açılış, üç ekran oranı ve sekiz sonuç dili. |
| 3 | Sonraki Bölüm, Tekrar Oyna, Haritaya Dön | Tamamlandı ve Unity'de doğrulandı | Gerçek tıklama, çift dokunuş ve eski bölümü yeniden oynama. |
| 4 | Premium düğmeler | Tamamlandı ve Unity'de doğrulandı | Roller, gerçek tıklama alanı, kilitli durum ve Klasik Ödüller girişi. |
| 5 | Mağaza ve altın | Tamamlandı ve Unity'de doğrulandı | Alışveriş, yetersiz ve negatif bakiye, çift işlem, kayıt hatası ve toparlanma. |
| 6 | Sahip olunanlar ve kullanım | Tamamlandı ve Unity'de doğrulandı | Satın alınan kozmetiklerin kuşanılması, gerçek şişe efekti ve ipucu tüketimi. |
| 7 | Ödüllü reklam | Harici hesap/servis bağlantısı gerekiyor | Test sağlayıcısı ve SSV imza kontrolleri geçti. Cihazda AdMob/UMP ve canlı SSV açık. |
| 8 | Klan | Harici hesap/servis bağlantısı gerekiyor | Gerçek yerel API ile hesap, lider, üyelik, istek, yetki ve kesinti. Üretim HTTPS açık. |
| 9 | Lig ve sezon | Harici hesap/servis bağlantısı gerekiyor | Gerçek yerel API ile hamle doğrulaması, 28 gün, kapanış, sıralama ve tek ödül. Üretim HTTPS açık. |
| 10 | Etkinlik merkezi ve ödüller | Harici hesap/servis bağlantısı gerekiyor | Gerçek yerel API ile 12 tür, takvim, klan katkısı, sandık ve makbuz. Üretim HTTPS açık. |
| 11 | Günlük görevler ve 24 saat | Tamamlandı ve Unity'de doğrulandı | Gerçek sayaç, tekrar açılış, 25 saat sonrası yenileme ve tek teslim. |
| 12 | Dünya haritaları | Tamamlandı ve Unity'de doğrulandı | Ada sınırları, güvenli alan, düğüm girişleri ve harita dönüşü. |
| 13 | Kristal Şelaleler | Tamamlandı ve Unity'de doğrulandı | 91–100 görünümü ve dört su bölgesinde kare farkı. |
| 14 | Kedi maskotu | Tamamlandı ve Unity'de doğrulandı | Haritada dinlenme, uyanma ve yürüyüş. Oynanışta portre alanı. |
| 15 | İlk dört bölüm öğreticisi | Tamamlandı ve Unity'de doğrulandı | Gerçek şişe etkileşimi, tekrar giriş, geçerli öneri ve beşinci bölüm sınırı. |
| 16 | iOS ve Android | Uygulandı, cihaz testi bekliyor | Mevcut ayarlar ve eski geliştirme paketi kayıtları incelendi. Yeni aday paket doğrulanmadı. |

## Kanıtlar

**Birleşik yeni oyuncu akışı iki Play Mode oturumunda 142 + 24 kontrolle geçti.** Temiz kayıt Boot'tan açıldı. İlk pati yardımı gösterildi ve 1–3 gerçek döküşlerle tamamlandı. Oyuncu ilk kazanışta yıldız ve altın aldı, Sonraki Bölüm ile ilerledi, mağazadan ipucu aldı ve gerçek bulmacada kullandı. İki kazanışlık günlük ödül alındı. Gerçek yerel hesapla klan kuruldu, üçüncü kazanış etkinlik ve lig puanı yazdı, etkinlik ödülü alındı. İlk Play Mode kapatıldı. İkinci Play Mode aynı yalıtılmış kayıtla açıldı ve yeniden girişten sonra klan üyeliği, lig puanı, etkinlik makbuzu, günlük ödül, altın, ipucu stoku ve açık 4. bölüm doğrulandı. Her iki oturumda oyuncunun asıl kayıt anahtarları değişmedi. [İlk oturum](Integrated/Phase-1.json) · [İkinci oturum](Integrated/Phase-2.json)

**Bölüm üretimi.** Bölüm 3–1026 arası 1.024 çözüm sertifikası yeniden oynatıldı. 39 seçili bölüm arşivsiz yeniden üretimde aynı düzeni verdi. Komşu tekrar 0, toplam kanonik benzersiz düzen 972. Üretim P95'i Editör makinesinde 10.84 ms, ölçülen iki Editör Boot → Home geçişi 2,18 ve 1,66 saniye. [Üretici denetimi](Integrated/Generator.json)

Aşağıdaki on Play Mode probunda toplam **2.628 kontrol** geçti. Birleşik akış dahil toplam **2.794 kontrol**.

Son Console örneğinde derleme başarısızlığı ve aktif hata yoktu. Çalışan Play Mode problarında hata kaydı oluşmadı. Derlemede mevcut `PremiumButton.Start` gölgeleme uyarısı, çevrimiçi panelde `MewgicBody` üç nokta karakteri bulunmadığında TMP kırpma uyarısı görüldü. Editör bağlantı taraması önizleme sahnesi açarken aynı katmanda birden fazla global ışık için URP hata kaydı üretti. Harita ve Gameplay Play Mode akışları bunu yeniden üretmedi. [Console özeti](CONSOLE.json)

Unity Test Framework EditMode’da bir Addressables örnek testi buldu ve çalıştırdı. PlayMode koşusu sıfır kayıtlı testle tamamlandı. Oyun doğrulaması için repo içindeki Editör doğrulayıcıları ve gerçek Play Mode probları çalıştırıldı. Editör doğrulayıcılarında 268 sahne/prefab/varlık bağlantısı, 80 harita/kayıt modeli, 16 günlük sayaç ve 53 ek günlük regresyon kontrolü geçti. [EditMode](TestFramework-EditMode.json) · [PlayMode Test Framework](TestFramework-PlayMode.json) · [Bağlantı taraması](EditModels/BINDINGS.json) · [Harita kayıt modeli](EditModels/Map-MODEL_TESTS.json) · [Günlük kayıt modeli](EditModels/Gameplay-MODEL_TESTS.json) · [Günlük regresyon](EditModels/Gameplay-DAILY_REGRESSION.json)

| Unity Play Mode probu | Kontrol | Çıktı |
| --- | ---: | --- |
| Bölüm ilerlemesi | 437 | [Sonuç](LevelProgression/PLAY_MODE.json) |
| Sonuç ekranı ve test reklamı | 373 | [Sonuç](Victory/PLAY_MODE.json) |
| Premium düğmeler | 977 | [Sonuç](Buttons/PLAY_MODE.json) |
| Mağaza | 218 | [Sonuç](Shop/PLAY_MODE.json) |
| Klan | 165 | [Sonuç](Clans/PLAY_MODE.json) |
| Lig | 83 | [Sonuç](League/PLAY_MODE.json) |
| Etkinlikler | 88 | [Sonuç](Events/PLAY_MODE.json) |
| Oynanış ve günlük görev | 114 | [Sonuç](Daily/UNITY_TESTS.json) |
| Dünya haritası ve Kristal Şelaleler | 107 | [Sonuç](Map/UNITY_TESTS.json) |
| Pati öğreticisi | 66 | [Sonuç](Tutorial/UNITY_TESTS.json) |

- SQLite üzerindeki gerçek HTTP regresyonları klan için 29, lig için 23, etkinlikler için 37 kontrolü geçti. Sahte puan alanı sonuçları değiştirmedi. Geçersiz bulmaca kanıtı ve yetkisiz klan işlemleri reddedildi. Yarışan üyelik ve ödül talepleri çoğalmadı. [Klan API](test_http.txt) · [Lig API](test_league.txt) · [Etkinlik API](test_events.txt)
- Bu koşuda sekiz SSV protokol kontrolü geçti. Geçerli imza, değiştirilmiş sorgu, yanlış reklam birimi ve eşzamanlı aynı bildirim işlendi. Editör sağlayıcısı erken kapatma, ağ kesintisi, reklamsız ilerleme, reddedilen doğrulama ve çift ödeme senaryolarını çalıştırdı. [SSV](SSV_TESTS.txt)
- 9:16, 9:19.5 ve 9:20 portre, metin taşması, tıklama alanı, eksik varlık bağlantısı ve maske/animasyon kontrolleri ilgili problarda çalıştı. Kristal Şelaleler’de dört su bölgesinin ortalama RGB kare farkı 4,646–17,498, sabit taş yol kontrolü 0,193 ölçüldü. Donanım kare zamanı, düşük bellek ve cihaz soğuk açılışı kabul testinde ölçülecek. [Şelale kare analizi](Map/WATER_CHECK.json)

### Game View kareleri

- [İlk öğretici](Integrated/01-First-Tutorial.png) · [İlk zafer](Integrated/02-First-Victory.png) · [Satın alma](Integrated/03-Purchased.png)
- [100 altına tamamlanan reklam test ödülü](Victory/07-AdRewardClaimed.png) · [Açılan etkinlik sandığı](Events/05-Chest-Reveal-9x20.png) · [Kristal Şelaleler](Map/03-Crystal-100.png)
- [Klan salonu](Integrated/04-Clan.png) · [Lig puanı](Integrated/06-League.png) · [Kayıttan yeniden açılan 4. bölüm](Integrated/08-Restored-Level4.png)

## Yayın kapıları

1. **Klan, lig ve etkinlikler için üretim bağlantısı.** `Assets/Resources/Clans/Connection.json` boş. Yerel servis test edildi. Hazırlık ve üretim HTTPS uç noktalarıyla gerçek telefonlardan, iki hesapla kabul koşusu açık.
2. **Reklam için üretim bağlantısı ve bekleyen teslim.** Android/iOS üretim App ID ve reklam birimi, UMP ve HTTPS SSV doğrulaması yapılandırılmamış. Mevcut doğrulama sorgusu işlem belleğinde ve sınırlı süre çalışıyor. Uygulama tamamen kapanırsa henüz doğrulanmamış reklam bonusu yeniden başlatılmıyor. Bu açık, reklam etkin yayın için kapanmalı. Bölümün temel ödülü ve ilerlemesi ilk kazanışta kaydediliyor.
3. **Çevrimiçi bekleyen kanıt.** Başarılı ve sunucuda onaylanmış kazanışlar korunuyor. Ağ yokken bekleyen kanıt kuyruğu oturum belleğinde. Uygulama kapatılırsa gönderilmemiş kanıt sonraki oturuma taşınmıyor. Cihaz kabulünde kesintiden geri dönme ve uygulamayı zorla kapatma ayrı denenmeli.
4. **Cihaz kabulü.** Android ve iOS Editor modülleri kurulu. Android ADB listesi boş, Windows ortamında Xcode bulunmuyor. Önceki Android Development APK ve iOS Xcode ihracı reklam entegrasyonu revizyonuna ait. Bunlar güncel klan, lig ve etkinlik paketini doğrulamaz. [Platform denetimi](PLATFORM_STATIC.json) · [Android cihaz listesi](ANDROID_DEVICES.txt) · [Cihaz kabul senaryoları](DEVICE_QA.md)

## Kapsam ve tekrar çalıştırma

Üretim kodu, onaylı görseller ve dünya temaları üzerinde değişiklik yapılmadı. Yeni `ReleaseIntegrationProbe.cs` ve `release_unity_qa.py` yalıtılmış yeni oyuncu akışını tekrar çalıştırır. `ButtonSystemProbe.cs` etkinlik ekranı yerleştikten sonra raycast yapacak biçimde güncellendi. Bu düzeltme üretim UI davranışını değiştirmez. Problar oyuncunun mevcut kayıt anahtarlarının test öncesi ve sonrası aynı olduğunu denetledi.

Unity açıkken `dotnet build tools/ClanServer -c Release`, ardından `python tools/ClanServer/release_unity_qa.py` çalıştırılır. Bölüm üretim denetimi Unity menüsünde **Mewgic → QA → Validate Release Generator** ile çalışır. Önceki Android ve iOS kayıtları [reklam entegrasyon raporunda](../RewardedAds/REPORT.md) yer alır.

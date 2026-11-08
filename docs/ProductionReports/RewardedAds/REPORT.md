# MeWGic ödüllü reklam

8 Kasım 2026 · Unity 6000.3.27f2 · Teslim dalı `release/0.8.0-rc.1`

Projede bir reklam SDK'sı yoktu. Var olan sağlayıcı arayüzü ve bölüm kazanış makbuzu üzerinden resmi Google Mobile Ads Unity Plugin 11.6.0 ve UMP eklendi. SDK, Google'ın GitHub sürümünden alındı. Arşivin yayınlanan SHA-256 değeri doğrulandı. iOS bağımlılıkları Unity External Dependency Manager 2.1.0 ile Podfile’a yazılır. Android, SDK’nın kendi bağımlılık XML’indeki sürümleri Unity Gradle şablonundan alır. Bu, Android’de editörü bekleten eşzamanlı çözümleyici çağrısını devre dışı bırakır. SDK varlıkları `Assets/GoogleMobileAds` ve `Assets/Plugins` altında, oyun bağlantısı `AdMobRewardedVictoryAd.cs` dosyasında. [SDK sürümü](https://github.com/googleads/googleads-mobile-unity/releases/tag/v11.6.0) · [Google Unity kurulumu](https://developers.google.com/admob/unity/quick-start)

## Ödül ve ekran akışı

- Sadece ödüllü reklamı açıkça seçen oyuncu reklam görür. UMP onayı alınabiliyorsa SDK bir reklamı önceden yükler. Sonuç paneli yükleniyor, hazır değil ve çevrimdışı durumlarını gösterir. Reklam yüklenirken ve hata verdiğinde Sonraki Bölüm çalışır.
- SDK'nın kazanma bildirimi gelmeden altın yazılmaz. Tam ekran reklam kapanınca veya oynatma hatası döndüğünde bölüm kontrolleri yeniden açılır. Üretim doğrulaması arka planda devam edebilir. Sonraki bölüme geçildiyse onay, kazanılan bölümün kendi makbuzuna yazılır.
- Altın tutarını SDK belirlemez. Mevcut `EconomyConfig` ve `Balance.json` kullanılır. Örneğin 20 altın kaydedildiyse yalnız 80 eklenir. Play Mode'daki üç yıldızlı ilk kazanış performans bonuslarıyla 50 altın verdi, reklam 50 daha ekledi. Bakiye ve tüketilen makbuz aynı kayıt işlemiyle yazılır. Aynı kazanıştan ikinci bonus alınamaz.
- Onaylı panel dokusu, video ve altın ikonları, glow ve shimmer kullanılıyor. Alt başlık reklamın durumuna göre değişir. Bölüm, yıldız ve cüzdan kaydı aynı depoda korunur.

[Hazır panel](04-ThreeStars-900x2000.png) · [Yükleme başarısız](05-AdUnavailable.png) · [Çevrimdışı](06-AdOffline.png) · [100 altın alındı](07-AdRewardClaimed.png)

## Test ile üretim ayrımı

| Platform | Google test App ID | Google test Rewarded Ad Unit ID | Üretimde eksik |
| --- | --- | --- | --- |
| Android | `ca-app-pub-3940256099942544~3347511713` | `ca-app-pub-3940256099942544/5224354917` | Android App ID ve Rewarded Ad Unit ID |
| iOS | `ca-app-pub-3940256099942544~1458002511` | `ca-app-pub-3940256099942544/1712485313` | iOS App ID ve Rewarded Ad Unit ID |

Development Build resmi örnek kimlikleri seçer. Editor Play Mode ağ reklamı yerine yalıtılmış test sağlayıcısı kullanır. Üretimde `Assets/Resources/Ads/Rewarded.json` içindeki `productionEnabled` kapalı. Bu dosyaya hedef platformun gerçek App ID ve Rewarded Ad Unit ID'si ile HTTPS `verificationBaseUrl` girilir. Ardından **Assets → Google Mobile Ads → Settings** altındaki native App ID eşleştirilir. Build doğrulaması test ve üretim kimliklerinin karışmasını engeller. UMP mesajları ve SSV geri çağrı adresi AdMob hesabında yapılandırılır. [Test reklamları](https://developers.google.com/admob/unity/test-ads) · [UMP](https://developers.google.com/admob/unity/privacy)

## Sunucu doğrulaması

`tools/RewardVerification` bağımsız .NET 10 doğrulama servisidir. HTTPS altında yayınlanır ve `AdMobUnits` ile virgülle ayrılmış üretim birimleri, `ReceiptDirectory` ile kalıcı yazılabilir dizin alır. AdMob'da her ödüllü birimin geri çağrı adresi `https://<doğrulama-sunucusu>/admob/ssv` olarak ayarlanır. Proxy geri çağrı sorgusunu değiştirmeden iletir. Reklamın `custom_data` alanı yalnız rastgele kazanış makbuzunu taşır. Sunucu Google ECDSA imzasını, birimi ve zamanı kontrol eder, makbuzu atomik biçimde bir kez kaydeder. Unity üretimde SDK'nın kazanma bildirimine ek olarak `/rewarded/{makbuz}` doğrulamasını bekler. Test reklamlarında sunucu koşulu kullanılmaz. [AdMob SSV](https://developers.google.com/admob/unity/ssv)

Sunucu yayına alınmadı ve AdMob hesabında canlı bildirim zinciri denenmedi. Doğrulama istemcisi sınırlı süre sorgular. Bu süre aşılırsa veya uygulama tamamen kapanırsa bekleyen reklam bonusu otomatik devam etmez. Oyuncunun önceden kaydedilmiş altını ve ilerlemesi korunur.

## Doğrulama

**Mewgic → QA → Run Rewarded Ads** menüsü gerçek Gameplay sahnesinde şişe dökerek sonuç ekranını açar. 373 Play Mode kontrolü sıfır Console hatasıyla geçti ve oyuncu kaydı korundu. Test sağlayıcısı başarı, erken kapatma, yükleme hatası, bağlantı kesilmesi, sunucu reddi, yinelenen tamamlama, reklamsız 1 → 2 ve doğrulama sürerken 1 → 2 senaryolarını çalıştırdı. 9:16, 9:19.5, 9:20, sekiz sonuç dili ve yalıtılmış kayıt kontrolü çalıştırıldı. [Play Mode](PLAY_MODE.json) · [Derleme](COMPILE.json) · [Sunucu testleri](SSV_TESTS.txt)

iOS Development Xcode ihracı sıfır hatayla tamamlandı. Info.plist örnek App ID'yi, Xcode projesi reklam köprüsünü ve Podfile AdMob ile UMP pod'larını içeriyor. Unity üç uyarı raporladı. Mevcut iOS App Info ve PremiumButton uyarılarına ek olarak Windows üzerinde CocoaPods çalıştırılmadı. Cihaz derlemesi için Mac'te `pod install` ve oluşan Xcode workspace kullanılır. [iOS ihracı](IOS_EXPORT.json)

Android Development APK sıfır hatayla üretildi. Paket `com.mewgic.game`, Android 36, ARM64 ve Google’ın örnek App ID’siyle doğrulandı. Derleme raporundaki tek uyarı mevcut `PremiumButton.Start()` uyarısı. APK yerel `Builds/QA/RewardedAds-Android.apk` konumunda. [Android APK doğrulaması](ANDROID_BUILD.json)

Fiziksel Android/iOS cihazı bağlı değildi. Gerçek AdMob test videosu ve canlı Google SSV bildirimi cihazda doğrulanmadı. Yalıtılmış imza kontrolleri ve yerel HTTP testleri 11/11 geçti. [Sunucu testleri](SSV_TESTS.txt)

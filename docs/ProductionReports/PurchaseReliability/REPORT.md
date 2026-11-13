# Güvenli satın alma ve ödül teslimi

**Canlı satın alma kapalı.** Unity IAP 5.5.0 akışı yalnız Unity Editor'da izole profille çalıştırıldı. Mobil mağaza bağlantısı, geliştirme derlemeleri dahil kapalı. Katalogdaki yedi SKU, paket miktarları ve mağaza görselleri değişmedi. Yeni mobil build alınmadı; mağazalarda ücretli işlem, satış veya dağıtım yapılmadı.

## Unity akışı

1. `PendingOrder` içindeki tek ürünün mağaza kimliği, işlemi ve adedi eşleşirse izole günlüğe yazılır. Geçici bağlantı hatasında ve ödeme beklerken ödül verilmez. İptal ve ödeme hatası işlem kimliği sağlanıyorsa günlüğe girer; mağaza kimlik üretmemişse yalnız durum arayüze yansır.
2. İzole doğrulayıcı, test ortamına ait kanıtı ve tekrarlar arasında değişmeyen işlem anahtarını döndürür. Altın/güçlendirici, teslimat anahtarı ve işlem durumunun güncellenmesi aynı `ProgressLedger` yazımına girer. Kayıt başarısızsa onay gönderilmez. Eski `storeDeliveries` elemanları korunur.
3. `ConfirmPurchase` yalnız teslimat kaydedildikten sonra asıl `PendingOrder` ile çağrılır. Onay sonucu eşleşen işleme uygulanır. Hata teslimi geri almaz; bekleyen işlem mağazadan tekrar geldiğinde veya durum denetlendiğinde yalnız onay denenir. Bir onay cevapsız kalırsa 30 saniye sonra durum denetimi yeniden denemeye izin verir. Apple'ın zaten tamamlanmış işlem cevabı, yalnız teslim edilmiş eşleşen işlem için başarı sayılır.
4. Günlükte platform, SKU, işlem kimliği, adet, işlem/ödül/onay durumları ve son UTC zaman damgası bulunur. Google'da Unity'nin işlem kimliği olarak taşıdığı ham token yerine SHA-256 özeti kullanılır; varsa sipariş numarası ayrıca tutulur. Makbuz, JWS, ham token veya mağaza hata detayları uygulama günlüğüne yazılmaz.

[Unity IAP 5 onay ve tekrar davranışı](https://docs.unity.com/en-us/iap/payment-providers/purchases-sdk) · [Unity makbuz doğrulaması](https://docs.unity.com/en-us/iap/receipt-validation)

## Backend ve hesaplar

ClanServer kullanıcıları ve oturumları SQLite/PostgreSQL'de saklıyor. Unity'nin ClanServer üretim URL'si boş. Oyunun altın ve güçlendirici bakiyesi cihazdaki `GameplayProgress` kaydında; ortak, sunucu yetkili oyun cüzdanı yok.

`005-purchases.sql`, doğrulanmış işlem ve hesabın altın/güçlendirici alacağını aynı veritabanı işlemine yazar. Tekillik platform + ortam + işlem kimliği üzerinden uygulanır. Aynı ürün ve adetle aynı hesaptan gelen tekrar mevcut teslimi döndürür; başka hesap, SKU veya adet değişikliği reddedilir. Hesap silinirse ödül kaydı silinir ve tekrar teslimi önleyen işlem satırı hesap bağlantısı olmadan kalır. Server kodu sadece izole test ortamına ait **önceden doğrulanmış** kayıtları kabul eder: `VerifiedStorePurchase` bir iç sözleşmedir. Genel IAP API'si, Apple/Google doğrulayıcısı ve oyun cüzdanına teslim henüz bağlı değildir. Canlı veritabanına geçiş çalıştırılmadı.

## Doğrulama

Unity 6000.3.27f2: **857 Play Mode kontrolü geçti**, Console hata/exception yok, gerçek oyuncu kaydı değişmedi. Derlemede mevcut `PremiumButton.Start` CS0114 uyarısı dışında sorun yok. [Play Mode sonuçları](PLAY_MODE.json)

| Senaryo | Sonuç |
|---|---|
| Başarılı satın alma ve kullanıcı iptali | Unity IAP Fake Store'da test edildi. Başarıda doğru ödül ve gerçek SDK onay bildirimi; iptalde bakiye sabit. |
| Aynı bildirimin gelmesi | Onay sürerken ve yeniden açılışta izole oynatım; ikinci ödül yok. SQLite'da iki eşzamanlı çağrı tek kayıt oluşturdu. |
| İnternet kesintisi ve bekleyen satın alma | Doğrulayıcı bağlantı hatası ve bekleyen sonuç ayrı ayrı simüle edildi; bakiye değişmedi, onay gitmedi. |
| Uygulama kapanması ve yeniden açılması | Teslim öncesi ve teslim sonrası kayıt yeniden diskten açılıp bildirim tekrarlandı. Fiziksel cihazda süreç sonlandırılmadı. |
| Teslim başarılı, mağaza onayı başarısız | Onay hatası kaydedildi; yeniden açılışta yalnız onay tekrarlandı. İlgisiz işlem kimliği onayı tamamlamadı. |
| Ödül kaydı başarısız | Unity kayıt hatasında bakiye ve teslim işareti değişmedi. SQLite teslim tablosu yazım hatası işlem satırını da geri aldı. |

`dotnet run --project tools/ClanServerPurchaseTests -c Release`: **15 kontrol geçti**. Sunucu ödülleri yedi Unity SKU'suyla eşleşti. `python tools/ClanServer/test_production.py`: **28 kontrol geçti**, mevcut hesap/oturum, yedekleme, silme ve eski şema geçişi dahil. PostgreSQL, Apple Sandbox ve Google lisans testi bu ortamda çalıştırılmadı.

Gerçek Unity Game View: [onay bekliyor](IapTestWaiting.png) · [izole satın alma başarılı](IapTestSuccess.png). Görsel varlık ve yerleşim değiştirilmedi.

## Canlı satıştan önce

1. Kimliği doğrulanmış hesapla çalışan, Apple/Google kanıtını sunucuda kontrol eden bir teslim API'si ekle. Apple JWS imzası ve App Store Server API ile `com.edasuyadik.mewgic`, ortam, SKU, ödeme/iptal durumu, adet ve `appAccountToken` eşleşmesini doğrula. Google Play Developer API'de paket, SKU, `PURCHASED`, adet, test ortamı ve `obfuscatedExternalAccountId` eşleşmesini doğrula. Mağaza yetkilerini ve anahtarları sunucu gizli depolarında tut. [Apple Server API](https://developer.apple.com/documentation/appstoreserverapi) · [Google satın alma yaşam döngüsü](https://developer.android.com/google/play/billing/lifecycle/one-time)
2. Ödül defterini sunucu yetkili cüzdan ve hesap envanterine bağla. Eski yerel ilerlemeyi koruyarak aynı hesapla ikinci cihazda teslim ve bakiyeyi doğrula. Yalnız kalıcı teslim cevabından sonra Unity onayı ver. Tekrar denemeleri, onay bekleyen işlemlerin uzlaştırılmasını, iadeleri ve kişisel veri saklama kurallarını tamamla.
3. App Store Connect'te aynı yedi Consumable, fiyat/metaveri, Paid Apps Agreement, vergi/banka durumu ve Sandbox hesabını doğrula. Google Play Console'da geliştirici erişimini, uygulama paketini, aynı yedi ürünü, Play Developer API yetkisini, dahili test ve lisans test hesaplarını doğrula. [Apple Sandbox](https://developer.apple.com/documentation/storekit/testing-in-app-purchases-with-sandbox) · [Google Play faturalama testleri](https://developer.android.com/google/play/billing/test)
4. Ayrı PostgreSQL test ortamında şema/yedekten dönüşü ve iki sunucu örneğiyle eşzamanlılığı çalıştır. Cihazda satın alma, gecikmeli ödeme, ağ kesintisi, zorla kapatma, iade ve ikinci cihaz senaryolarını tamamla. Canlı satış ancak bundan sonra ayrı bir yayın kararıyla açılabilir.

Değişen başlıca dosyalar: `StorePayments.cs`, `StorePackages.cs`, `BoosterActions.cs`, `GameplayProgress.cs`, `MagicShopProbe.cs`; ClanServer satın alma şeması, kayıt katmanı, geçiş ve test dosyaları. Bu teslim `release/0.8.0-rc.1` dalı içindir.

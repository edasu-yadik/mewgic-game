# iOS ve Android mağaza içi satın alma altyapısı

## Denetim

Unity 6000.3.27f2 projesinde önceki sürümde Unity IAP paketi veya mağaza teslimat kaydı bulunmuyordu. Paket kartlarında örnek TL fiyatları ve örnek indirimler vardı. Unity Player Settings, iOS için `com.edasuyadik.mewgic` ve Android için `com.mewgic.game` tanımlıyor. [Önceki TestFlight kaydı](../TESTFLIGHT_SETUP.md) iOS uygulama kaydını, [önceki Play Console denetimi](../Part24/PLAY_PREPARATION.md) geliştirici kayıt akışını belgeliyor. Bunlar mevcut IAP ürün durumunu doğrulamıyor. Salt okunur hesap kontrolü için başlatılan Computer Use oturumu tarayıcı adresini yeterli güvenle belirleyemediğinde otomatik durdu. Bu oturumda mağaza hesapları, ürünleri veya yetkileri doğrulanamadı.

## Uygulama

- Unity IAP 5.5.0 eklendi. [Paket kataloğu](../../../Assets/Resources/ShopReference/Offers.json) ürün miktarlarını, altın bonusunu, güç adetlerini ve ayrı `appleId` / `googleId` eşlemelerini içeriyor. Onaylanmış mağaza ürün kimlikleri gelene kadar eşlemeler boş.
- Altın Paketleri, Başlangıç Paketi, Oyun Gücü Paketi ve Özel Teklifler altındaki altın promosyonları mevcut kartlarda. Altın bonusları teslimata dahil. 10.000 altın ve %30 bonus toplam 13.000 altın verir. Örnek TL fiyatları, üzeri çizili fiyatlar ve doğrulanmamış indirim rozetleri kaldırıldı. Test oturumu varsa fiyat ve para birimi Unity IAP'ın yerelleştirilmiş fiyat dizesinden gösterilir. Diğer durumda kartlar önizleme açar.
- Başlangıç Paketi 1.000 altın, 3 Ek Şişe ve 2 Şişe Çekici içeriyor. Oyun Gücü Paketi 5 Ek Şişe, 3 Şişe Çekici, 3 Karıştır ve 1 Renk Değiştir içeriyor. Kullanılabilir karşılıklar seçildiği için eski önizlemedeki Ekstra Hamle ve Renk Bombası bu iki pakete dahil edilmedi.
- Mağaza işleniyor, onay bekliyor, başarılı, başarısız ve iptal edildi durumlarını gösteriyor. Doğrulama veya kayıt beklediğinde işlem yeniden denetlenebilir. İlk açılışta ve mağazaya yeniden girildiğinde bekleyen işlemler alınır.
- Doğrulayıcı ürün kimliğini, adedi, ödeme durumunu, uygulamayı, kullanıcıyı ve sandbox ortamını doğrulamalı, aynı işlem için aynı benzersiz kimliği vermeli. Yerel kayda makbuzun kendisi yerine platform ve işlem kimliğinden üretilen özet, ürün ve adet yazılır. Altın, stok veya kalıcı sahiplik ve teslimat işareti aynı kayıtla değişir. Unity IAP onayı kayıttan sonra gönderilir. Kayıt hatası işlemi açık bırakır.
- Kalıcı ürün tipi için mevcut kozmetik sahipliği ve doğrulanmış geri yükleme yolu hazır. Katalogda ücretli kalıcı ürün tanımlı değil. Mevcut sahip olunan kozmetikler korunur. Apple için manuel geri yükleme çağrısı, Android için satın almaları yeniden sorgulama çağrısı mevcut.
- Cihaz entegrasyonunun giriş noktası `StorePayments.ConfigureNativeSandbox`. Geliştirme iOS/Android derlemesi, test hesabına bağlı doğrulayıcı ve platform ürün kimlikleri gerektirir. Editör yerel test oturumunu izole profilde kullanır. Cihaz sandbox teslimatları hesap bazında ayrı kayıt açar. Canlı satın alma başlatacak bir giriş çağrısı eklenmedi.

## Mağaza hesabı ve cihaz sandbox adımları

1. **App Store Connect.** Uygulama erişimi olan hesapla In-App Purchases bölümünü, ücretli uygulama sözleşmesi durumunu ve sandbox test kullanıcısını doğrula. Paketleri sarf edilebilir ürünler olarak, varsa kalıcı hakkı ayrı bir sarf edilmeyen ürün olarak tanımla. Yerel fiyat ve bölgeleri ürün kaydından yönet. Yetkili hesapla sonraki bir test turunda sandbox cihazı kullan. [Apple IAP kurulumu](https://developer.apple.com/help/app-store-connect/configure-in-app-purchase-settings/overview-for-configuring-in-app-purchases/) · [Apple sandbox](https://developer.apple.com/help/app-store-connect/test-in-app-purchases/overview-of-testing-in-sandbox)
2. **Google Play Console.** `com.mewgic.game` uygulaması ve yetkili ödeme profiliyle tek seferlik ürünleri, satın alma seçeneklerini, dahili test kanalını ve lisans test kullanıcılarını doğrula. Gecikmeli onay ve gecikmeli ret için Play'in test ödeme araçlarını kullan. [Play Billing testi](https://developer.android.com/google/play/billing/test)
3. Doğrulanmış ürün kimliklerini kataloğa gir ve geliştirme açılışına kimlik doğrulamalı test servisini bağla. Apple StoreKit 2 imzalı JWS ve Google Play satın alma makbuzunu sunucuda doğrula. Hesap, ortam, uygulama, SKU, adet ve işlem durumunu denetle. Canlı kullanımdan önce sunucuda işlem kimliğine benzersiz kısıt koy ve teslimatı hesapla ilişkilendir. [Unity makbuz doğrulaması](https://docs.unity.com/en-us/iap/receipt-validation) · [Unity sipariş yaşam döngüsü](https://docs.unity.com/en-us/iap/purchases)

Mağaza konsollarında ürün, ayar, ödeme, yayın veya erişim işlemi yapılmadı. Yeni iOS/Android derlemesi alınmadı.

## Unity Play Mode ve paket çözümleme

Unity 6000.3.27f2’de izole mağaza ve ekonomi turunda 760 kontrol geçti. Kayıt yuvaları değişmedi. Yerel test sağlayıcısı ile fiyat/para birimi, işleniyor, iptal, hata, gecikmeli onay, üretim ortamı makbuzunun reddi, iOS/Android ürün eşlemesi, ürün adedi, yeniden açılışta tekrar bildirim, yazma hatasında atomiklik ve kalıcı sahiplik kontrol edildi. Gerçek Apple ve Google cihaz sandbox testleri hesap erişimi, ürün kimlikleri ve doğrulayıcı bağlantısı bekliyor.

- [Play Mode raporu](PLAY_MODE.json)
- [Yerel test fiyatı](IapTestLocalizedPrice.jpg)
- [Onay bekleniyor](IapTestWaiting.jpg)
- [Başarılı yerel teslimat](IapTestSuccess.jpg)

Unity Package Manager bu oturumda resmî 5.5.0 kayıt deposu indirmesine HTTP 401 döndürdü. Testler Unity'nin resmî arşiviyle geçti. Arşivin SHA-1 değeri kayıt deposundaki `00a2945f1a91ab88ed74741e26cd64ef5f29399d` ile eşleşti. GitHub paket bağımlılığı standart `5.5.0` kaydını kullanır. Mevcut yerel Editor, `Library/IapPackageInspection/iap.tgz` üzerinden doğrulanmış paketi kullanır. Unity Hub oturumunu düzelttikten sonra commit'teki `Packages/manifest.json` ve `Packages/packages-lock.json` sürümlerine dönüp Package Manager çözümlemesini tekrar çalıştırabilirsin. [Unity yerel arşiv kurulumu](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PackageManager.Client.Add.html)

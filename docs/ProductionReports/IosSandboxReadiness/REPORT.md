# iOS Sandbox satın alma test hazırlığı

Unity Editor hazırlığı tamamlandı. **Mevcut iOS derlemesi cihazda satın alma testine hazır değil.** `ConfigureNativeSandbox` kapalı; Apple JWS doğrulaması ve doğrulanmış ödülü ayrı test hesabına kaydedecek backend bağlantısı henüz yok. App Store Connect oturumu giriş ekranına döndü. Bu nedenle ürünlerin Apple tarafında varlığı, Sandbox erişimi, fiyatları ve sözleşme durumu doğrulanamadı. Hesapta değişiklik yapılmadı.

## Katalog ve fiyat

Unity 6000.3.27f2 ve resmi Registry paketi `com.unity.purchasing` **5.5.0** doğrulandı. iOS bundle kimliği `com.edasuyadik.mewgic`; repo teslim yapılandırması uygulama Apple ID'sini `6826795393` olarak kaydediyor. Tablodaki kimlikler Unity'nin **Apple** eşlemesidir. Yedi ürünün hepsi Consumable; üç önizleme kartı katalog SKU'su değildir.

| Apple Product ID | Tek satın alımın toplam ödülü | Unity Editor satın alma | Apple Sandbox / gerçek fiyat |
|---|---|---|---|
| `com.mewgic.game.gold_250` | 250 altın | Fake Store geçti | Doğrulanmadı |
| `com.mewgic.game.gold_1000` | 1.100 altın (%10 dahil) | Fake Store geçti | Doğrulanmadı |
| `com.mewgic.game.gold_2500` | 2.875 altın (%15 dahil) | Ürün getirme geçti | Doğrulanmadı |
| `com.mewgic.game.gold_5000` | 6.000 altın (%20 dahil) | Ürün getirme geçti | Doğrulanmadı |
| `com.mewgic.game.gold_10000` | 13.000 altın (%30 dahil) | Fake Store geçti | Doğrulanmadı |
| `com.mewgic.game.starter` | 1.000 altın, 3 Ek Şişe, 2 Şişe Çekici | Fake Store geçti | Doğrulanmadı |
| `com.mewgic.game.powers` | 5 Ek Şişe, 3 Şişe Çekici, 3 Karıştır, 1 Renk Değiştir | Fake Store geçti | Doğrulanmadı |

Fiyat gösterimi Unity IAP'ın ürün metaverisindeki `localizedPriceString` alanına bağlı; ISO para birimi ve pozitif fiyat da doğrulanıyor. Bağlantı kesilir, ürün bulunamaz veya fiyat geçersizse satın alma düğmesi devreye girmez. Fake Store'daki USD ve simüle edilen EUR fiyatları gerçek App Store fiyatı değildir. Gerçek fiyat, Apple Sandbox ürün getirme sırasında seçili mağaza bölgesi için alınacak.

## Unity Editor sonuçları

**883 Play Mode kontrolü geçti.** Beş ürünün her biri Apple SKU eşlemesiyle Unity IAP **Fake Store** üzerinden alındı. Onaydan sonra aynı işlem bildirimi tekrar işlendi; altın ve güçlendiriciler ürün başına tam bir kez eklendi. Fake Store iptali ve reddedilen ödeme bakiye yazmadı. Doğrulama beklerken ödül verilmedi. Ağ ve kayıt hataları simüle edildi. Kapanma senaryosunda ayrı test kaydı diskten yeniden açılıp bekleyen işlem tekrar verildi; hem teslim öncesi hem teslim sonrası sınırları kontrol edildi. İşletim sistemi seviyesinde kapanma iPhone testine kaldı.

Gerçek oyuncu kayıt anahtarı ve yedekleri değişmedi. Testler ayrı geçici Unity profili ve izole Fake Store defteri kullandı. Console hata/exception yok. Derlemede mevcut `PremiumButton.Start` CS0114 uyarısı kaldı. [Play Mode kanıtı](PLAY_MODE.json)

900 × 1600 gerçek Unity Game View: [Yükleniyor](IosIapLoading.png) · [Ürün bulunamadı](IosIapMissing.png) · [İptal](IosIapCancelled.png) · [Başarı](IapTestSuccess.png) · [Bağlantı hatası](IosIapConnectionError.png). Bağlantı hatası görseli simüle edilen arayüz durumudur. Satın alma görselleri ve paket içerikleri değiştirilmedi.

## TestFlight / iPhone kontrol listesi

- [ ] App Store Connect'e yetkili hesapla giriş yap. MeWGic → Monetization → In-App Purchases altında yedi Consumable kimliğini, yerelleştirmeyi, fiyatı ve hedef mağaza bölgesini doğrula. Business altında Paid Apps Agreement etkin olmalı; vergi/banka durumunu kontrol et. Meta veri değişiklikleri Sandbox'a yansımak için bir saate kadar sürebilir. [Apple Sandbox ürün erişimi](https://developer.apple.com/documentation/technotes/tn3186-troubleshooting-in-app-purchases-availability-in-the-sandbox)
- [ ] Users and Access → Sandbox bölümünde ayrı bir Sandbox Apple Account kullan. Yeni hesap gerekiyorsa daha önce Apple Account veya mağaza alışverişi için kullanılmamış adres ve test edilecek ülke/bölge seç. Ayrı bir MeWGic QA hesabı kullan. [Apple Sandbox hesabı](https://developer.apple.com/help/app-store-connect/test-in-app-purchases/create-a-sandbox-apple-account)
- [ ] Test sürümüne Apple JWS doğrulaması, Sandbox ortamı, bundle, SKU, işlem, adet ve `appAccountToken` doğrulaması ile izole sunucu cüzdanı bağlanmalı. Doğrulayıcı veya test hesabı hazır değilse mağaza açılmamalı; üretim makbuzu reddedilmeli. Test hesabının ödemesi hiçbir koşulda yerel oyuncu cüzdanına yazılmamalı. [Önceki güvenli teslim raporu](../PurchaseReliability/REPORT.md)
- [ ] Uygun iOS test sürümünü iPhone'a kur. TestFlight kullanılırsa Apple'ın kendi Sandbox hesabıyla test etme adımlarını izle: TestFlight uygulamasını indir, Medya ve Satın Almalar oturumundan çık, Ayarlar → Geliştirici → Sandbox Apple Account altında test hesabıyla giriş yap. Geliştirme imzalı bir sürüm de Apple Sandbox'ı destekler; TestFlight tek seçenek değildir. Mevcut kilitli sürüm testi başlatmaz. [Apple TestFlight adımları](https://developer.apple.com/help/app-store-connect/test-a-beta-version/testing-subscriptions-and-in-app-purchases-in-testflight/)
- [ ] Mağazayı aç. İstenen yedi SKU'yu, gelen ürün sayısını, yerelleştirilmiş fiyatı ve para birimini kaydet. Aynı bölgenin Apple satın alma ekranıyla karşılaştır. Yüklenirken, eksik SKU ve bağlantı hatasında fiyat veya satın alma seçeneği gösterilmemeli. Bağlantı geri gelince Tekrar Dene'yi kontrol et.
- [ ] Tablodaki beş paketi ayrı ayrı al. Her işlemde önce/sonra altın ve güçlendirici farkını, doğrulama ortamını ve işlem/teslim/onay durumlarını kaydet. Doğrulama tamamlanana kadar bakiye sabit, teslimden sonra fark tablodaki gibi olmalı. Makbuz veya kimlik bilgilerini test raporuna koyma.
- [ ] Apple ödeme ekranından iptal et; bakiye ve teslim kayıtları değişmemeli. Sandbox hesap ayarında **Allow Purchases & Renewals** kapalıyken reddedilen satın almayı dene; bakiyeyi ve hata mesajını kontrol et, sonra ayarı geri aç. Gerekirse **Interrupt Purchases** ile ayrı bekleyen işlem senaryosu çalıştır. [Apple ödeme hatası testi](https://developer.apple.com/documentation/storekit/testing-failing-subscription-renewals-and-in-app-purchases) · [Sandbox hesap ayarları](https://developer.apple.com/help/app-store-connect/test-in-app-purchases/manage-sandbox-apple-account-settings/)
- [ ] Bir işlemi ödeme tamamlanmadan, bir işlemi de teslim kaydedilip mağaza onayı sürerken zorla kapat. Gerekirse ikinci sınırı test backend'iyle geciktir. Yeniden açılışta mağazadan gelen tekrar bildirimini bekle; aynı işlemin ödülü en çok bir kez verilmeli. Ağ kesip açarak tekrarla. Gerçek oyuncu cüzdanını ve test cüzdanını ayrı ayrı doğrula.

Değişiklikler `StorePayments.cs`, `MagicShopPanel.cs` ve `MagicShopProbe.cs` ile sınırlı. Bu çalışmada iOS build alınmadı, Mac'e dosya gönderilmedi, TestFlight'a yükleme yapılmadı, Apple Sandbox veya canlı ödeme başlatılmadı. Teslim dalı `release/0.8.0-rc.1`.

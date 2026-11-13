# Unity IAP 5.5.0 ürün kataloğu

13 Kasım 2026 · Unity 6000.3.27f2 · release/0.8.0-rc.1

`Assets/Resources/ShopReference/Offers.json` mağaza kartlarını ve çalışma zamanında `StoreController.FetchProducts` çağrısına verilen ürün tanımlarını besler. Önceki yedi iç ürün kimliği korundu. Apple ve Google alanları boştu. İki platforma aynı kalıcı kimlikler verildi ve yedi ürün `Consumable` olarak kaydediliyor. Altın sekmesindeki iki 10.000 Altın kartı aynı ürünü kullanıyor.

| Unity kimliği | Apple ve Google ürün kimliği | Görünen ad | Teslim edilen içerik |
| --- | --- | --- | --- |
| gold_250 | com.mewgic.game.gold_250 | 250 Altın | 250 Altın |
| gold_1000 | com.mewgic.game.gold_1000 | 1.000 Altın | 1.000 + 100 bonus = 1.100 Altın |
| gold_2500 | com.mewgic.game.gold_2500 | 2.500 Altın | 2.500 + 375 bonus = 2.875 Altın |
| gold_5000 | com.mewgic.game.gold_5000 | 5.000 Altın | 5.000 + 1.000 bonus = 6.000 Altın |
| gold_10000 | com.mewgic.game.gold_10000 | 10.000 Altın | 10.000 + 3.000 bonus = 13.000 Altın |
| starter | com.mewgic.game.starter | Başlangıç Paketi | 1.000 Altın, 3 Ek Şişe, 2 Şişe Çekici |
| powers | com.mewgic.game.powers | Oyun Gücü Paketi | 5 Ek Şişe, 3 Şişe Çekici, 3 Karıştır, 1 Renk Değiştir |

Altın başlığında taban miktar, rozette ilave yüzde ve ayrıntıda teslim edilecek toplam görünür. Bonus teslimatta bir kez hesaplanır. Özel Teklifler altındaki 100, 250 ve 1.000 Altın önizlemeleri satış kapsamına alınmadı. Referanstaki Ekstra Hamle için oyunda hamle sınırı ve kullanılabilir güç bulunmadığından mevcut Unity kataloğundaki Ek Şişe korundu. Referanstaki Karıştırıcı ve Renk Bombası, çalışan Karıştır ve Renk Değiştir güçlerine karşılık gelir. Onaylı PNG, atlas, sahne ve prefab dosyalarına dokunulmadı.

## Bağlantı ve teslimat

`StorePackages.cs` katalog açıklamasını taşır. `StorePayments.cs` mağaza yanıtından yerelleştirilmiş fiyat metnini, ISO para birimini, başlığı ve açıklamayı alır. Ürün eksikse önceki fiyat silinir ve alım başlatılamaz. `MagicShopPanel.cs` mağaza metaverisini satın alma penceresinde gösterir ve eksik açıklamada katalog metnini kullanır. Referanstaki TL fiyatları kullanılmadı. `EconomyConfig` oyun içi altın ve elmas fiyatlarını yönetmeye devam eder.

`StoreController` bağlantı, ürün ve satın alma getirme, iptal, başarısızlık, ertelenmiş işlem ve mağaza onayı olaylarını işler. Bekleyen satın alma doğrulanınca ödül ve kanonik işlem kaydı birlikte yazılır. Ardından mağazaya onay gönderilir. Onay hatasında işlem beklemede kalır. Yeniden bildirimde kayıt eşleşirse tekrar onay denenebilir ve ikinci ödül verilmez. Kalıcı hakların mevcut geri yükleme yolu korunur.

Cihaz sandbox giriş noktası geliştirme derlemesi ve enjekte edilen bir doğrulayıcı gerektirir. Yerel testte Unity Fake Store ve izole kayıt kullanıldı. Apple ve Google cihaz sandbox'ı için mağaza ürünleri ve platform makbuzlarını doğrulayan bir sağlayıcı hâlâ gerekli. Bu turda yeni mobil derleme, tahsilat, canlı satın alma veya mağaza kayıt değişikliği yapılmadı.

## Unity doğrulaması

844 Play Mode kontrolü geçti. Test yedi SKU'nun iki platform eşlemesini, doğru altın ve güçlendirici adetlerini, geçersiz kimliği, mağazadan eksilen SKU'yu, gerçek IAP Fake Store iptalini ve onayını, gecikmeli doğrulamayı, tekrarlanan işlemi ve yeniden açılan kaydı kapsadı. Oyuncu kayıt anahtarları önceki değerleriyle eşleşti. Play Mode hata sayısı sıfır. Derleme hatası sıfır, mevcut `PremiumButton.Start` CS0114 uyarısı sürüyor.

- [Play Mode çıktısı](PLAY_MODE.json)
- [Özel Teklifler](Offers.png)
- [Altın Paketleri](IapTestLocalizedPrice.png)
- [Yerelleştirilmiş test fiyatı ve toplam teslimat](IapLocalizedDetails.png)
- [Oyun Güçleri](Powers.png)
- [Kostümler](Costumes.png)

Dört sekmenin Game View çıktısında düzen ve sanat onaylı referanslarla karşılaştırıldı. Test fiyatı, ödeme durumu, bonusun ilave olduğunu belirten artı işareti ve çalışan güçlerin adları ekrandaki metin farklarıdır. Testler 900 × 1600, 900 × 1950 ve 900 × 2000 portre boyutlarını kapsadı. Fiziksel cihazda doğrulama yapılmadı.

## App Store Connect

MeWGic → Monetization → In-App Purchases bölümünde yedi kayıt oluşturulur. Her satır için tür Consumable seçilir, Product ID alanına tam kimlik girilir ve referans adı atanır. Yerelleştirilmiş ad ve açıklama tablodaki içerikle eşleşmelidir. Fiyat kademesi, bölge kullanılabilirliği ve App Review ekran görüntüsü ayrı ayrı eklenir. İlk IAP gönderimi, hazır olduğunda uygulama sürümüyle birlikte yapılır. [Apple ürün oluşturma kılavuzu](https://developer.apple.com/help/app-store-connect/manage-in-app-purchases/create-consumable-or-non-consumable-in-app-purchases/) · [Apple inceleme kılavuzu](https://developer.apple.com/help/app-store-connect/manage-submissions-to-app-review/submit-an-in-app-purchase/)

İlgili katalog, kod ve doğrulama kanıtları `release/0.8.0-rc.1` teslimine dahildir. Önceden var olan yerel değişiklikler commit kapsamı dışındadır.

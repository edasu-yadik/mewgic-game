# App Store Connect IAP hazırlığı

Unity iOS Bundle ID'si `com.edasuyadik.mewgic`. Önceki App Store Connect kurulum kaydı ve iOS teslim yapılandırması MeWGic için **Apple ID `6826795393`** değerinde eşleşiyor. App Store Connect bu oturumda Apple giriş ekranını açtı. Bu nedenle canlı uygulama kaydı, ürünlerin varlığı ve Business durumları doğrudan doğrulanamadı. Ürün taslağı oluşturulmadı. [TestFlight kurulum kaydı](../TESTFLIGHT_SETUP.md) · [iOS teslim yapılandırması](../../../BuildConfig/iOS-TestFlight.json)

Unity IAP 5.5.0 kataloğu yedi Consumable ürünü içeriyor. Aşağıdaki Apple Product ID'leri Unity'deki değerlerle karakter karakter aynı. Apple tarafında mevcut bir ürünü Product ID ile ara. Yalnız eksik kaydı oluştur. Özel Teklifler'deki üç küçük altın önizleme kartı bu yedi SKU'ya dahil değildir. [Unity kataloğu](../../../Assets/Resources/ShopReference/Offers.json)

| Product ID | Product type | App Store Connect durumu | Yerelleştirme | Fiyat | Sandbox | Eksik işlem ve inceleme görseli |
|---|---|---|---|---|---|---|
| `com.mewgic.game.gold_250` | Consumable | Doğrulanamadı | TR / EN hazır | Doğrulanamadı | Apple cihazında doğrulanmadı | Kimliği eşleştir veya taslak aç. Metin, fiyat ve kullanılabilirliği denetle. [Game View](Review/Gold.png) |
| `com.mewgic.game.gold_1000` | Consumable | Doğrulanamadı | TR / EN hazır | Doğrulanamadı | Apple cihazında doğrulanmadı | Kimliği eşleştir veya taslak aç. Metin, fiyat ve kullanılabilirliği denetle. [Game View](Review/Gold.png) |
| `com.mewgic.game.gold_2500` | Consumable | Doğrulanamadı | TR / EN hazır | Doğrulanamadı | Apple cihazında doğrulanmadı | Kimliği eşleştir veya taslak aç. Metin, fiyat ve kullanılabilirliği denetle. [Game View](Review/Gold.png) |
| `com.mewgic.game.gold_5000` | Consumable | Doğrulanamadı | TR / EN hazır | Doğrulanamadı | Apple cihazında doğrulanmadı | Kimliği eşleştir veya taslak aç. Metin, fiyat ve kullanılabilirliği denetle. [Game View](Review/Gold.png) |
| `com.mewgic.game.gold_10000` | Consumable | Doğrulanamadı | TR / EN hazır | Doğrulanamadı | Apple cihazında doğrulanmadı | Kimliği eşleştir veya taslak aç. Metin, fiyat ve kullanılabilirliği denetle. [Game View](Review/Gold.png) |
| `com.mewgic.game.starter` | Consumable | Doğrulanamadı | TR / EN hazır | Doğrulanamadı | Apple cihazında doğrulanmadı | Kimliği eşleştir veya taslak aç. Metin, fiyat ve kullanılabilirliği denetle. [Game View](Review/Offers.png) |
| `com.mewgic.game.powers` | Consumable | Doğrulanamadı | TR / EN hazır | Doğrulanamadı | Apple cihazında doğrulanmadı | Kimliği eşleştir veya taslak aç. Metin, fiyat ve kullanılabilirliği denetle. [Game View](Review/Offers.png) |

## Yerelleştirme

Her iki dilde görünen ad 2–30, açıklama en çok 45 karakter. Referans adı olarak Türkçe görünen ad kullanılabilir. İngilizce için English (U.S.), Türkçe için Turkish seçilebilir. Altın açıklamalarındaki toplamlar bonusu içerir. Oyun Gücü Paketi tam teslimatı 5 Ek Şişe, 3 Şişe Çekici, 3 Karıştır ve 1 Renk Değiştir içerir. İki özel teklif mevcut oyundaki Ek Şişe'yi verir. [Apple IAP alanları](https://developer.apple.com/help/app-store-connect/reference/in-app-purchases-and-subscriptions/in-app-purchase-information)

| Unity kimliği | Türkçe ad | Türkçe açıklama | İngilizce ad | İngilizce açıklama |
|---|---|---|---|---|
| `gold_250` | 250 Altın | 250 Altın. | 250 Gold | 250 Gold. |
| `gold_1000` | 1.000 Altın | %10 bonus dahil toplam 1.100 Altın. | 1,000 Gold | 1,100 Gold total, including a 10% bonus. |
| `gold_2500` | 2.500 Altın | %15 bonus dahil toplam 2.875 Altın. | 2,500 Gold | 2,875 Gold total, including a 15% bonus. |
| `gold_5000` | 5.000 Altın | %20 bonus dahil toplam 6.000 Altın. | 5,000 Gold | 6,000 Gold total, including a 20% bonus. |
| `gold_10000` | 10.000 Altın | %30 bonus dahil toplam 13.000 Altın. | 10,000 Gold | 13,000 Gold total, including a 30% bonus. |
| `starter` | Başlangıç Paketi | 1.000 Altın, 3 Ek Şişe ve 2 Şişe Çekici. | Starter Pack | 1,000 Gold, 3 Extra Bottles, 2 Bottle Hammers |
| `powers` | Oyun Gücü Paketi | 5 Ek Şişe, 3 Çekiç, 3 Karıştır, 1 Renk Takası | Power Pack | 5 Bottles, 3 Hammers, 3 Shuffles, 1 Swap |

## Game View inceleme dosyaları

[Altın Paketleri](Review/Gold.png) beş altın SKU'sunu, [Özel Teklifler](Review/Offers.png) iki paket ve içeriklerini gösterir. İki PNG, mevcut izole Unity Game View çekimlerinin tam kadrajlı, RGB, 1242 × 2208 piksel kopyalarıdır. Orijinal görüntü boyutu 900 × 1600. Yeni çözünürlükte doğrudan çekim denemesinde Play Mode sona erdiği için mevcut doğrulanmış çekimler aynı en-boy oranıyla ölçeklendi. Görsellerin içeriği düzenlenmedi. Kareler mağaza fiyatı bulunmadığında kullanılan Önizleme durumunu gösterir. Apple yükleme formu erişilebilir olduğunda dosya kabulü ayrıca doğrulanmalı. [Apple ekran görüntüsü ölçüleri](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications)

İnceleme notu için İngilizce metin.

> Open the Magic Shop from Home. Gold products are under the Gold Packs tab and the Starter and Power packs are under Special Offers. Gold card titles show the base amount. The bonus badge indicates extra gold. Pack contents are shown in the offer cards.

## Fiyat, kullanılabilirlik ve hesap koşulları

1. MeWGic → **Monetization → In-App Purchases** bölümünü aç. Product ID ve türü eşleştir. Yalnız eksik kayıtları Consumable taslak olarak oluştur. Hazır yerelleştirmeleri ve ilgili Game View dosyasını Review Information alanına ekle. [Apple ürün oluşturma](https://developer.apple.com/help/app-store-connect/manage-in-app-purchases/create-consumable-or-non-consumable-in-app-purchases/)
2. Her ürünün fiyat çizelgesinde baz ülke, para birimi, seçilen fiyat noktası ve etkili tarihi doğrula. Onaylı mağaza görsellerindeki TL değerleri fiyat kararı olarak kullanılmadı. Yeni taslaklarda canlı satış kapalı kalacaksa Apple'ın **Remove from Sale** kullanılabilirlik seçeneğini kullan. Mevcut ürünlerin satış ayarını denetle. [Apple fiyat noktaları](https://developer.apple.com/help/app-store-connect/manage-in-app-purchases/set-a-price-for-an-in-app-purchase/) · [Apple kullanılabilirliği](https://developer.apple.com/help/app-store-connect/manage-in-app-purchases/set-availability-for-in-app-purchases/)
3. **Business** altında Paid Apps Agreement, Bank Accounts ve Tax Forms durumlarını kontrol et. Apple, ücretli IAP ve sandbox için sözleşmenin yürürlükte olmasını ister. Eksik banka veya vergi durumu sandbox'ta ürünlerin gelmesini engelleyebilir. Hesaba ait bu üç durum giriş ekranı nedeniyle doğrulanamadı. [Apple sandbox teşhis kılavuzu](https://developer.apple.com/documentation/technotes/tn3186-troubleshooting-in-app-purchases-availability-in-the-sandbox)
4. Mağaza metaverisi tamamlandığında Sandbox Apple Account ile ürün getirme ve test işlemini doğrula. Cihaz testinin mevcut Unity akışı geliştirme derlemesine ve doğrulayıcıya bağlı. Önceki 844 kontrol Unity Fake Store'da geçti. Apple Sandbox cihaz testi bu turda çalıştırılmadı. [Unity doğrulaması](../IapProductCatalogue/REPORT.md) · [Apple Sandbox](https://developer.apple.com/help/app-store-connect/test-in-app-purchases/overview-of-testing-in-sandbox)

Teslim kapsamı belge ve Game View dosyalarıdır. Unity kataloğunda veya diğer oyun dosyalarında değişiklik, yeni mobil build, ücretli işlem ya da canlı satış işlemi yapılmadı.

# MeWGic Büyülü Mağaza ve altın ekonomisi

8 Kasım 2026 · Unity 6000.3.27f2 · Başlangıç `8d186fd` · Teslim dalı `release/0.8.0-rc.1`

Ana menüde eski Envanter konumu artık **Mağaza**. Üst altın sayacındaki mağaza simgesi de aynı ekranı açıyor. Mor-altın başlık, altın bakiyesi, beş kategori ve Sahip Olduklarım görünümü mevcut onaylı UI ailesini kullanıyor. Ürün kartları mevcut oyun görsellerini, ürünün işlevini, stok veya kuşanma durumunu ve alış fiyatını gösteriyor.

[Satın alma akışı · 700 → 660 altın](SHOP_FLOW.png) · [Güçlendiriciler](01-Boosters.png) · [Onay](02-Confirm.png) · [Kostümler](03-Costumes.png) · [Şişeler](04-Bottles.png) · [Efektler](05-Effects.png) · [Yetersiz bakiye](06-Insufficient.png) · [Sahip Olduklarım](07-Owned.png) · [Özel Paketler](08-Packs.png) · [9:20 güvenli alan](09-Shop-SafeArea.png) · [Kuşanılan kedi](10-EquippedHome.png) · [Kuşanılan cam](11-EquippedGameplay.png) · [Gerçek şişe parıltısı](12-EquippedSparkle.png)

## Başlangıç dengesi

Değerler `Assets/Resources/Economy/Balance.json` üzerinden `EconomyConfig` tarafından okunur. Altın, mağazada kazanılan ve harcanan para birimidir. Eski mücevher ve enerji alanları kayıt uyumluluğu için korunur.

| Kaynak | Altın | Diğer |
| --- | ---: | --- |
| Yeni oyuncu | 0 | 3 Geri Al, 3 İpucu, 1 Ek Şişe, 1 Karıştır |
| İlk bölüm kazanışı | 20 temel | 1 İpucu |
| Yeni en iyi yıldız | Yıldız başına +5 | Eski en iyi skor yalnız artar |
| İlk denemede ilk kazanış | +10 | — |
| İpucusuz ilk kazanış | +5 | — |
| Onaylanan ödüllü reklam | Toplam 100’e tamamlayan fark | İlk kazanış başına bir hak |
| Günlük sandık | 100 | 1 İpucu |
| Günlük görev · Şişeler / Kazanış / Yıldız / Güçlendirici | 15 / 25 / 30 / 10 | Göreve ait 1 mevcut güçlendirici |
| Haftalık etkinlik | 60 | 1 Karıştır, Yıldız Rozeti ve Gül Parıltısı |
| Sezon eşikleri | 30 / 45 / 75 | Mevcut güçlendiriciler, finalde Kedi Kartı ve Altın Cam |

| Kategori | Ürün ve fiyat |
| --- | --- |
| Güçlendiriciler | Geri Al 35 · Ek Şişe 60 · İpucu 40 · Karıştır 50. Her alışta 1 kullanım |
| Kedi kostümleri | Klasik dahil · Kristal Büyücü 240 · Altın Büyücü 360 · Gece Yıldızı 300 |
| Şişe tasarımları | Klasik dahil · Kristal Cam 150 · Altın Cam 240 |
| Büyülü efektler | Altın Parıltı dahil · Gül Parıltısı 120 · Kristal Parıltısı 180 |
| Özel Paketler | Bilgi kategorisi. Satış ve ödeme eylemi bulunmuyor |

Geri Al yalnız geçerli bir önceki döküşü geri çevirir. Ek Şişe bulmaca başına en fazla bir boş şişe ekler. İpucu doğrulanmış çözümün bir döküşünü gösterir. Karıştır, renkleri koruyarak şişeleri yeniden sıralar ve çözücü kontrolü başarısızsa stok tüketmez. Mevcut bölümlerde hamle sınırı bulunmadığı için Ekstra Hamle örneği yerine Karıştır kullanıldı.

## Kayıt ve kuşanma

Satın alma onayı, ürün kimliğini, gösterilen fiyatı ve kayıt içindeki bir sonraki işlem numarasını taşır. Kayıt günlüğü bakiyeyi, ürünü ve işlem numarasını aynı işlemde yazar. Yazma başarısızsa üçü de eski hâlinde kalır. Başarılı işlem numarası yeniden kullanılamaz. Kozmetik tekil olarak satın alınır. Güçlendirici ayrı bir onayla tekrar alınabilir.

Eski kayıtlar `shop` alanı olmadan açılır. Mevcut altın, mücevher, enerji, yıldız, ödül ve stok korunur. Önceden kazanılmış Yıldız Rozeti ve Kedi Kartı ilgili yeni kozmetik hakkını verir. Kozmetik kuşanma sahiplik doğrulamasıyla saklanır. Kedi renk seçenekleri mevcut şapka ve pelerin çizimini kullanır. Cam rengi iksirlerin paletini değiştirmez. Efekt rengi mevcut büyü ve şişe parıltılarını kullanır. Kostümler ana menü ve Gameplay maskotunda çalışır. Harita ve onaylı bölüm sonu kompozisyonu kendi sanatını kullanır.

## Unity’de deneme

Normal akışta `Boot.unity` → **Oyna** veya alt menüden **Mağaza**. Yeterli altınla bir ürüne bas ve satın almayı onayla. Kozmetiği Mağaza veya Sahip Olduklarım içinden kuşan. Görseldeki 700 altın, yalıtılmış QA profili için başlangıç bakiyesidir. Yeni oyuncu yapılandırması sıfır altınla başlar.

Otomatik akış için Play Mode dışındayken **Mewgic → QA → Run Magic Shop**. Bu akış gerçek Home, Boot ve Gameplay sahneleri, UI raycast ve izole PlayerPrefs kayıtlarıyla çalışır.

Unity 6000.3.27f2 üzerinde [218 mağaza kontrolü](PLAY_MODE.json), [70 ekonomi regresyonu](ECONOMY_REGRESSION.json) ve [975 premium düğme regresyonu](BUTTONS_REGRESSION.json) geçti. Mağaza koşusunda 9:16 ve 9:20 portre, bozuk ana kayıttan yedek geri yükleme, tekrar edilen ödeme isteği, kuşanılan kostüm/cam/efekt ve gerçek bölüm kazanışı doğrulandı. Üç koşu da asıl oyuncu kaydının korunduğunu bildirdi.

Gerçek ödüllü reklam sağlayıcısı ve App Store / Google Play ödemeleri bu aşamada devreye alınmadı. Kayıt yeniden açma Unity Editor’da gerçek kayıt günlüğü ve Boot üzerinden doğrulandı. Bağımsız uygulama kapat-aç, fiziksel cihaz ve dağıtım build’i bu denetime dahil değildi.

## Ana kaynaklar

- `Assets/Scripts/Core/EconomyConfig.cs`, `Assets/Resources/Economy/Balance.json`, `Assets/Scripts/Presentation/UnityEconomyConfig.cs`
- `Assets/Scripts/Core/MagicShop.cs`, mevcut ilerleme ve ödül defterleri, `Assets/Scripts/Presentation/GameplayProgress.cs`
- `Assets/Scripts/Presentation/MagicShopPanel.cs`, ana menü bağlantıları, mağaza yerelleştirmeleri ve mevcut premium düğme sistemi
- `Assets/Scripts/Presentation/ShopCosmetics.cs`, `Assets/Resources/UI/ShopCostume.shader`, kedi ve şişe sunum bağlantıları
- `Assets/Scripts/Tests/MagicShopProbe.cs`, satın alma onayına uyarlanan mevcut QA ve bu klasördeki Game View çıktıları

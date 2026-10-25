# PART 06A — Fantastik dünya haritası

25 Ekim 2026 • Unity 6000.3.27f1 • C:/Dev/Mewgic

## Yapılanlar

LevelMap'in önceki kamera/ışık yapısı korundu; yalnız bu sahneye MapWorld eklendi. Runtime Canvas üzerinde ayrı gökyüzü, dört bölge zemini, yollar, 12 seviye konumu, dört sandık, etiketler, navigasyon, sis ve ışık parçacıkları oluşturulur. Tek ekran resmi kullanılmaz. Bölgeler ayrı sprite ve Transform'lardır; sandıklar ve konumlar bağımsız Button bileşenleridir.

Enchanted Village sıcak ışıklı evler/çeşme/çiçekler; Crystal Falls mavi kristaller/şelaleler/köprüler; Moon Garden mor ağaçlar ve parlayan köşk; Sky Castle bulutlar içinde altın-mavi kale içerir. Dikey ScrollRect dokunma sürüklemesi ve fare tekerleğini destekler; dört bölge sekmesi yumuşak kaydırır. Ada, düğüm, sandık ve etiket birlikte hafifçe hareket eder. Sis/parçacıklar yavaş hareket eder; şelale renklerinde ince ışık akışı ve sıcak pencerelerde düşük genlikli titreşim vardır. Bu, akış simülasyonu değildir.

Konum ve sandıklar açıkça önizlemedir: tıklama bilgi paneli açar; bölüm başlatmaz, sandık ödülü vermez, oyuncu ilerleme/kayıt verisini okumaz veya değiştirmez. Map içinden Ana Menü dönüşü çalışır. Kullanıcının Home'u değiştirmeme sınırı nedeniyle HomeNavigation.mapSceneReady ve Home sahnesi korunmuştur; Home'dan harita girişinin bağlanması sonraki parta bırakılmıştır.

## Dosyalar ve sanat üretimi

Assets/Art/Environment/Map/ altında Map_Village_v001.png, Map_Crystal_v001.png, Map_Moon_v001.png, Map_Castle_v001.png, Map_Sky_v001.png, Map_Chest_v001.png, Map_Node_v001.png ve MapProps.spriteatlas oluşturuldu. Onaylı 03_Map.png referansıyla ImageGen üretildi; Blender kullanılmadı. İstem kayıtları PART06A_PROMPTS.md dosyasındadır. Sky opak; diğer altı PNG gerçek alpha içerir. Crystal kaynağında dış kenarda 17 düşük alpha pikseli vardır; Game View'da belirgin dikdörtgen kenar görülmedi.

Yeni kod: Assets/Scripts/Map/MapWorld.cs, MapPoint.cs, MapPathGraphic.cs; Assets/Shaders/MapWaterLight.shader; Assets/Editor/MapSceneBuilder.cs, MapWorldTests.cs. Mevcut Home sis sprite'ı, Home navigasyon ikonu ve Gameplay mor-altın panel sanatı yalnız kaynak olarak yeniden kullanıldı; dosyaları değiştirilmedi.

## Gerçek doğrulama

- Unity Editor'a gerçek MCP eval çağrılarıyla bağlanıldı. Native CLI aracının Pipeline örneği bulunmadığından mevcut EditorMCP stdio köprüsü kullanıldı; yalnız dosya taramasıyla Editor testi yapılmış sayılmadı.
- C# derlendi; LevelMap gerçek Play Mode'da açıldı. MapWorldTests: 556 kontrol başarılı (PART06A_TEST_RESULTS.json).
- Dört bölge geçişi, 12 konum/dört sandığın tamamında gerçek EventSystem raycast/tıklama, bilgi paneli kapatma, Map→Home, sürükleme/tekerlek kaydırması, katman ayrımı ve çevre saati test edildi.
- 900x1600, 900x1950, 900x2000, çentik/alt inset benzetimi ve 360x640 yerleşim testleri geçti. Oranlar korunur, UI viewport'a girmez, dokunma alanları en az44px ve ayrı konum hit bölgeleri çakışmaz.
- Mevcut 12 model kontrolü ve izole profilli Boot→Home→Gameplay→Home smoke testi geçti; Undo/Restart/kazanma/tekrar oynama/animasyon kilidi doğrulandı.
- Console: 0 hata, 0 uyarı. PART06A_FINAL_UNITY.json son Editor durumunu kaydeder.
- 260 mevcut dosyanın SHA256 karşılaştırmasında yalnız LevelMap.unity değişti. Home, Gameplay, onaylı karakter/şişe/logo, çekirdek ve bölüm verileri korundu (PART06A_PRESERVATION.json).

## Görsel kanıtlar

- PART06A_GAMEVIEW.png: gerçek Unity ScreenCapture, LevelMap Play Mode, 900x1600 alt bölgeler.
- PART06A_SKY_GAMEVIEW.png: gerçek Unity ScreenCapture, üst bölgeler.
- PART06A_COMPARISON_Dark.png / Light.png: onaylı referans ile bu iki gerçek görüntü yan yana; geçici kaydedilmeyen karşılaştırma sahnesinde oluşturuldu.
- PART06A_LAYOUT_*: oran ve Safe Area testlerinde Unity kameradan RenderTexture çıktılarıdır; fiziksel cihaz ekran görüntüsü değildir.

## Mobil kaynaklar ve sınırlar

Bölgeler/gökyüzü en fazla2048, konum/sandık en fazla512; alpha, sRGB, bilinear, clamp; Read/Write ve mipmap kapalı. Android ASTC4x4; iki tekrar kullanılan simge 1024 atlasında, rotation/tight packing kapalı. PART06A_TEXTURE_AUDIT.json Editor ölçümlerini içerir; Editor texture belleği cihaz GPU belleği gibi sunulmaz. Üretim PNG toplamı yaklaşık15MB; bölge dokuları ön yüklenir. Fiziksel Android cihazda FPS/bellek/ısı ve dokunma testi yapılmadı; APK üretilmedi. Uzun haritalarda bölge yükleme/boşaltma ileride gerekebilir.

## Referansa kalan farklar

Renk ayrımı, ada yönleri, mor bahçe ve altın-mavi kale referansın sanat diline yakındır. Ancak kompozisyon birebir değildir: dört bölge kaydırılan daha uzun ve seyrek düzendedir; bölgeler arasındaki boşluk daha fazladır. Ada mimarisi yeniden üretilmiştir; yollar referanstaki sürekli taş patika kadar organik değildir ve bölgeler arası ince büyülü bağlantıdır. Şelalelerin gerçek katmanlı akış deformasyonu yerine ışık titreşimi vardır. UI etiketleri genel mor-altın paneli kullanır; referanstaki özel ahşap tabelalar değildir. Numara/yıldız/kilit/ödül/ilerleme, üst ekonomi, yan etkinlikler ve beşli alt navigasyon bu kapsamda bağlanmadı. Kedi haritaya eklenmedi.

Bu sürüm test edilmiş çevre ve yerleşim altyapısıdır; referansın birebir nihai sanat eşleşmesi olarak kabul edilmez. Sonraki partın seviye ilerleme bağlantısına teknik olarak hazırdır. Belirtilen görsel farklar final sanat onayından önce değerlendirilmeli.

## Git

Yalnız PART06A dosyaları seçilerek commit/push yapılır. Önceden bulunan SourceArt, eski şişe deneyleri ve diğer untracked çalışmalar dahil edilmez. Commit ve uzak SHA doğrulaması teslimde kaydedilecektir.

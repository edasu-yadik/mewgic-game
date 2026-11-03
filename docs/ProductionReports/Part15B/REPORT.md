# PART 15B · Kristal Şelaleler

4 Kasım 2026 · Unity 6000.3.27f1 · Başlangıç `d6ba5c5`

## Uygulama

- Kristal Şelaleler'in 51–100 rotası için beş bağımsız 1024×1536 teras çizimi. Döşeme ve kemerli köprüler artık aynı perspektifte boyanmış arazinin parçası. Teras sınırlarında dar bir saydam geçiş kullanılıyor. 91–100 kesitinde kale, pembe ağaçlar, fenerler, kristaller ve birden çok kotta şelale var.
- Bölüm taşları, beş ayrı çizimde el ile ölçülen rota konumlarına bağlanan Unity düğmeleri. Altın kilometre taşı 100, kayıttaki güncel durum için hareketli ok ve yumuşak ışık, kilit simgesi ve gerçek yıldız sayıları ayrı sunuluyor.
- Mevcut büyücü kedi dokusu, çizimde onun için ayrılmış kaya düzlüğünde temas gölgesi ve sıcak tonla yerleştirildi.
- Şelale hareketi ikinci doku örneği ve aşağı kayan vurgu ile, kristal parıltısı düşük genlikli renk maskesiyle çalışıyor. Altı küçük parçacık ve güncel taşın yumuşak ışığı Play Mode’da canlanıyor. Bir ortak malzeme kullanılıyor. Beş doku Android ve iPhone için ASTC 5×5 ayarlı. Editor Profiler beş doku için toplam 34,8 MiB raporladı. Bu Editor ölçümüdür. Cihaz GPU, RAM ve FPS ölçümü yapılmadı.
- Bölüm sonunda arka plan kararıyor ve tıklamaları kesiyor. Alttaki Harita, Yeniden ve durum metni gizleniyor. Tekrar Oyna ile geri geliyor.

## PART 15A ile karşılaştırma

| Alan | PART 15A | PART 15B |
| --- | --- | --- |
| Patika ve köprü | Düz programatik şerit, ada resminin üzerinde | Taş döşeme ve köprüler araziyle aynı perspektifte |
| Zemin | Aynı Kristal Şelaleler çizimi tekrar ediyordu | Beş farklı çizim ve elle hizalanmış rota. Teraslar arasındaki dekor geçişi kaydırırken hâlâ seçilebiliyor. |
| Kedi | Gökyüzü önünde bağımsız bir öğe gibi duruyordu | Kaya çıkıntısına temas gölgesiyle oturuyor. Orijinal dokunun stil farkı yakın bakışta görülebilir. |
| Bölüm sonu | Panelde ve oyun HUD’ında iki Harita düğmesi | Sonuç panelinde tek Harita düğmesi |

[Önceki görünüm](../IslandJourney/GameView.png) · [Unity 91–98](Native_Kristal_91_98.png) · [Unity 92–100](Native_Kristal_92_100.png) · [Bölüm sonu](Native_Bolum_Sonu.png)

Önceki sohbetin `/mnt/data/a_highly_detailed_colorful_fantasy_game_level_map.png` referansı bu oturumda erişilebilir değildi. Renk paleti, kademeli şelaleler, kemerler, kale ve kedi kompozisyonu yazılı tariften uyarlandı. Referans PNG ile piksel veya kompozisyon eşleşmesi doğrulanmadı.

## Unity Editor doğrulaması

- [Play Mode denetimi](UNITY_TESTS.json) 7.264 kontrol, 0 hata ve 0 uyarı. Gerçek oyuncu ana kaydı, yedek ve bozuk kayıt alanları aynı kaldı. Erken denemede testte eksik namespace giderildi ve dışarıdan değişen LevelMap sahnesi Editor’da yeniden yüklendi.
- 12 dünya adasındaki 600 bölüm ve 601–650 devam aralığı tarandı. Yıldız, kimlik, kilit, kaydırma, tuş odağı, yeniden boyutlandırma ve 48 px dokunma hedefleri denetlendi. Gözlenen en yüksek etkin taş sayısı 12.
- 900×1600, 900×1950 ve 900×2000 gerçek Game View denetimleri yapıldı. Ayrıca [360×640 kompakt](Compact_360x640.png) ve [900×2000 çentik paylı](SafeArea_900x2000.png) kamera çıktıları alındı. İki doğal Game View karesi birlikte 91–100 aralığını kapsıyor.
- Şelale materyal saati ve aktif ok hareketi çalıştı. Aynı kaydırma konumundaki [ikinci su karesinde](Native_Akis_B.png) sağ şelale alanındaki 4.400 pikselin 2.572’si değişti. 100 Gameplay sahnesine girdi, dokuz hamlede kazanıldı, tek Harita düğmesi doğrulandı ve tekrar oynatmada oyun kontrolleri geri geldi.
- Yeni build oluşturulmadı. Diğer adaların görsel rotaları kendi mevcut sunumunu kullanıyor. 12 ada, ada başına 50 bölüm, prosedürel katalog ve save şeması korunuyor.

## Dosyalar

`Assets/Art/Environment/Map/CrystalJourney` beş teras dokusu ve import ayarlarını içerir. `Assets/Scenes/LevelMap.unity` doku referanslarını taşır. `MapWorldCrystalJourney.cs` teras, temas gölgesi, parçacık ve rota noktalarını bağlar. `MapWorldJourney.cs`, `JourneyRouteGraphic.cs` ve `MapWorldProgression.cs` mevcut düğme havuzunu ve taş sunumunu kullanır. `MapWaterLight.shader` yeni akış çeşidini, `GameplayEndScreen.cs` ve `PrototypeApp.cs` sonuç katmanını yönetir.

[ImageGen promptları ve üretim notları](ART_PROMPTS.md)

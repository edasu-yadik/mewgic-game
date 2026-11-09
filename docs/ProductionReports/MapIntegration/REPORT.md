# Dünya ve bölüm haritası entegrasyonu

## Kök neden ve düzeltme

- Ana menünün Harita düğmesi ile Kristal Visual QA kısayolu aynı `Assets/Scenes/LevelMap.unity` sahnesini açıyordu. `MapWorld.Mount` olağan açılışta `FocusCurrent()` ile seçili adanın bölüm katmanını hemen öne getiriyordu. Yeni oyuncu bu yüzden dünya seçimini görmeden Büyülü Köy 1–10'a giriyordu. Ana menü açılışı artık dünya haritasında kalıyor. Bekleyen ilerleme yürüyüşü ilgili teras seçildiğinde gösteriliyor.
- `LevelMap.unity` içinde `useLegacyJourneyArt: 0`, beş Kristal teras dokusu, şelale shader referansı ve kedi yürüyüş atlası yerinde. `Assets/Resources/Map/JourneyThemes/crystal.asset` mevcut `Crystal_Reference_91_100.png` dokusuna ve dinlenme atlasına bağlı. Referans `CrystalTerrace_5` için kullanılıyor. 51–90 mevcut dört ayrı terasını kullanmaya devam ediyor. 91–100 artık oklarla görülebilir. Teras gezinmesi bölüm açmaz ve yıldız vermez.
- Büyülü Köy'ün uzun şeridini ortak `JourneyRouteGraphic` çiziyordu. Geçici 1–10 düzeni mevcut doku, kısa köprüler ve geçiş taşları kullanıyor. Bölüm taşları, maskot ve pati izi aynı rota koordinatlarını kullanıyor. Diğer adalara bu geçici görünüm uygulanmadı.
- Dünya kartının durum hesabı ve açtığı teras artık aynı grup indeksini kullanıyor. Geri dönüşte aynı ada odakta kalıyor. Her iki katman bölüm kilitleri ve yıldızlar için aynı `MapProgression` ve `ProgressLedger` verisini kullanıyor.

## Katmanlar ve kaynaklar

| Görünüm | Kullanılan kaynak |
| --- | --- |
| Dünya haritası | `Map_Crystal_v001.png` gibi ada sprite'ları, `Map_Sky_v001.png`, `MapWaterLight.shader` ve mevcut ada kartları |
| Kristal Şelaleler 51–90 | `Crystal_Terrace_01–04.png`, teraslara ait mevcut taş ve rota koordinatları |
| Kristal Şelaleler 91–100 | `Crystal_Reference_91_100.png`, `MEWGIC_CRYSTAL_REFERENCE` shader varyantı, `ReferenceWalk` koordinatları |
| Kedi | `MapCat_Poses.png`, `CrystalCat_RestWake` ve canlı pati izi mesh'i |
| Büyülü Köy bölüm haritası | `Journey_Paving.png`, mevcut gökyüzü ve kristal katmanları, paylaşılan teras ve geçici köprü mesh'i |

## Game View karşılaştırması

- [Dünya haritasında Kristal Şelaleler](World-Map.jpg)
- [Gerçek oyun rotasından 91–100, yeni oyuncu kaydında kilitli](Crystal-From-World.jpg)
- [Visual QA 91–100, 100. bölüm açık ve kedi dinleniyor](Crystal-QA-1600.png)
- [99 → 100 yürüyüşü ve altın pati izleri](Crystal-Walking.jpg)
- [Büyülü Köy, geçici 1–10 yerleşimi](Village-1-10.jpg)
- [9:19.5](Crystal-QA-1950.jpg) ve [9:20](Crystal-QA-2000.jpg)

Referanstaki kıvrımlı köprüler, sağ üst kale, katmanlı şelaleler, sağ pembe ağaç, fener ve kristaller aynı kompozisyonda. Bölüm taşları ile yıldızlar canlı UI. Alt gezinme, başlık ve sayfa göstergeleri referanstaki dekorla birebir aynı değil. Altın pati izleri yürüyüş sırasında beliriyor.

## Unity doğrulaması

Unity 6000.3.27f2'de C# derlemesi hata vermedi. `Verify Full Game Preview` Play Mode denetiminde 29 kontrol geçti. Yeni oyuncu kaydıyla Ana Menü → Dünya Haritası → Kristal Şelaleler → 51–60 → 91–100 yolu gerçek UI tıklamalarıyla işletildi. Kilitli 100. bölüm oyunu açmadı. Visual QA'nın 100. bölümü açık örneğinde aynı texture ve on düğüm konumu gözlendi. Teras okları 900 × 1600, 1950 ve 2000 portre boyutlarında test edildi. [Visual QA sonucu](VISUAL_QA.json)

Dünya haritasından 90 → 91 geçişi kaynak terasta başladı ve referans terasında tamamlandı. 99 → 100 yürüyüşünde sırasıyla kıvrılma, göz açma, esneme, ayağa kalkma, iki adım pozunun dönüşümlü oynatılması, altın pati mesh'i ve kıvrılarak dinlenme gözlendi. Kalıcı oyuncu ve kurtarma anahtarları testten önce ve sonra byte düzeyinde aynıydı. Edit Mode kayıt modelinde 80 kontrol geçti. [Kayıt modeli sonucu](MODEL_TESTS.json)

Mevcut Map Flow Play Mode denetiminde 105 kontrol geçti. Şelale shader saati, katmanların kaydırma davranışı, pati izlerinin viewport içinde oluşması, az hareket modu, 12 dünya sınırı ve 51. bölümden oyun ekranına geçiş doğrulandı. İki Play Mode raporunda hata listesi boş. [Harita regresyon sonucu](MAP_FLOW_TESTS.json)

## Dosyalar

- `Assets/Scripts/Presentation/HomeNavigation.cs`
- `Assets/Scripts/Map/MapWorld.cs`
- `Assets/Scripts/Map/MapWorldJourney.cs`
- `Assets/Scripts/Map/MapWorldCrystalJourney.cs`
- `Assets/Scripts/Map/MapWorldJourneyLayers.cs`
- `Assets/Scripts/Map/MapWorldJourneyMotion.cs`
- `Assets/Scripts/Map/JourneyRouteGraphic.cs`
- `Assets/Scripts/Tests/FullGamePreview.cs`
- Bu rapor, Play Mode kanıtları ve `docs/PRODUCTION_PROGRESS.md`.

Unity sahnesi, prefab, sprite, texture, shader, oyuncu kayıt formatı ve bulut yapılandırmalarında değişiklik yok. Büyülü Köy'ün nihai sanatı kullanıcı onayı bekliyor. Fiziksel cihaz testi yapılmadı ve mobil build alınmadı.

Teslim dalı `release/0.8.0-rc.1`. Değişiklikler bu dalda sürümlendi.

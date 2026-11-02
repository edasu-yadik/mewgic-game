# PART13F — Clockwork Isles / Saat Adaları

2 Kasım 2026 · Unity6000.3.27f1 · recovery/part09-completion

## Sonuç ve kapsam

Yalnız dokuzuncu ada, **401–450**, üretildi. AGENTS.md, PART13A mimarisi, son13E raporu, Git branch/değişiklikleri, gerçek03_Map ve onaylı Crystal Falls sprite incelendi. Başlangıç HEAD:b1ab0abcdf4d6512ac486a74ee09ed96f0a471bd. Unity MCP bağlantısı doğrulandı; işlemler aynı bağlı Editor/Pipeline7800 üzerindeki Unity CLI ile yapıldı. CLI güncellenmedi. LevelMap gerçek sahnesi kullanıldı.

Önce sanat planı ve dört bağımsız kaliteli PNG üretildi. **Yerleşik image_gen** kullanıldı; tam promptlar GENERATION_PROMPTS.md içinde. Ekteki küçük örnek görsel üretim girdisi veya ayrı talimat kaynağı olarak alınmadı. Blender/render modeli, stok varlık, CLI/API fallback veya nihai primitive ada kullanılmadı. Önceki sekiz ada korundu; sonraki adalara başlanmadı. **Yeni Player/iOS/IPA/TestFlight/Cloud veya Addressables içerik build'i alınmadı.**

## Sanat ve ayrı bileşenler

| Kaynak, Assets/Art/Environment/Map/Islands/clockwork altında | İçerik | Kaynak / runtime |
|---|---|---|
| IslandBase/Clockwork_Island_v001.png | Taş/altın uçan kaya, mor-mavi enerji kristalleri, mekanik süslü köprüler, ön rota, bitkiler/fenerler |1254² /1254² |
| Architecture/Clockwork_Tower_v001.png | Ayrı büyük saat kulesi, dekoratif kadran/saat sembolleri, sıcak pencereler |1254² /1024² |
| Architecture/Clockwork_Workshops_v001.png | Ayrı küçük fantastik atölye kümesi, tezgâhlar, çiçekler ve ışıklı pencereler |1254² /512² |
| Decorations/Clockwork_Gear_v001.png | Altın/mor/mavi tek önden görünüş dişli sprite'ı;3 bağımsız instance tarafından paylaşılır |1254² /512² |

Taban ve kule kaynak kopyaları SourceArt/Map/Islands/clockwork/Renders altında da saklandı; diğer kaynaklar proje içindeki PNG'lerdir. Kaynak boyutları2048 olarak sunulmadı; importer üst sınırı kaynak boyutu değildir. Dört PNG'de sıfır alpha ve yumuşak kenar var; alpha>8 olan tuval kenarı pikseli **0** (ALPHA_AUDIT.json). Dişli delikleri gerçekten saydamdır. Büyük gerçek render ve gerçek UI zemini üzerinde kenar/siluet/ölçek incelendi.

Assets/Prefabs/Map/ClockworkIsles.prefab PART13A'nın dokuz katman sırasını korur: Background, IslandBase, Architecture, Vegetation, WaterOrSpecialEffects, Decorations, LevelNodes, Rewards, ForegroundEffects. Taban/kule/atölyeler/dişliler bağımsız Image/RectTransform;3 enerji geçişi WaterOrSpecialEffects altında. Küçük köprü/çiçek/fener/su ayrıntıları tabana bake edilmiştir, her ayrıntı ayrı3D obje değildir. Mevcut düğümler ve sandık MapWorld sahibinde bağımsız etkileşimli UI olarak kalır; görsellere gömülmez.

İlk gerçek önizlemede kule ucu üstteki komşu ada paneline yaklaştı; yalnız Clockwork prefabında kule340→320 UI birimine küçültüldü. Dişli merkezleri y=.20→.215 ile isim plakasından uzaklaştırıldı. Son kabul ve ekran görüntüleri bu yerleşimle alındı.

## Animasyon ve performans yaklaşımı

ClockworkIslandPresentation üç merkez pivotlu dişliyi **±2.5°/sn** yavaş ters yönlerde döndürür; tam tur yaklaşık144sn. Tek sprite paylaşılır,0.85sn hafif alpha/ölçek girişi ve ayrı kristal/saat/pencere ışığı saati vardır. ClockworkIslandLayers.shader statik ana görseli değiştirmeden kaynak atlasları paylaşan düşük alpha mor/mavi enerji ve sıcak altın ışık geçişini çizer. Kaynak piksellerindeki alpha korunur; dikdörtgen efekt arka planı yok. Materyal örneği instance yok edilirken bırakılır.

Dişli dönüşleri dekoratiftir: Rigidbody/Rigidbody2D, kuvvet, çarpışma, gerçek diş teması veya fizik simülasyonu yok. Kadran statik süs sanatıdır, gerçek saati gösteren yeni oyun mekaniği değildir. Ayrı su akışı simülasyonu eklenmedi. Sanat raycastTarget=false, CanvasGroup blocksRaycasts=false; düğüm/sandık dokunmasını engellemez.9 Image toplamı ve3 küçük transform dönüşü kullanılır; parçacık fırtınası, karmaşık Animator/3D rig veya fizik zinciri yoktur.

## Harita ve üretici bağlantısı

IslandArtSlot'a yalnız clockwork desteği eklendi. Map/clockwork, ayrı yerel Map-ClockworkIsles grubundaki tek prefab adresidir; Local.BuildPath/LoadPath, PackTogether ve IncludeInBuild=true. Önceki gruplar ve Localization ayarları korunur. Geç gelen yükleme nesil kontrolü ve release yolu aynı; ilk dört resident sprite dizisine yeni referans eklenmedi. Testler AssetDatabase Play Mode üzerinden yapıldı; paketlenmiş bundle/Player kabulü yapılmadı.

MapWorld5 görünüm/40 MapPoint havuzu aynıdır;600 aktif bölüm nesnesi yoktur. Clockwork odağında komşu Coral/Frost ile3 sanat handle/instance bulunur. Bu3 sayısı bütün haritadaki tüm odakların global üst sınırı iddiası değildir; ortak5 slot sözleşmesi korunur. Yalnız Clockwork grup ve sandık ankrajları uyarlandı; önceki adaların ankrajları değişmedi.

World.json tek anlamsal farkı **clockwork.artReady false→true**.401–450, ödül kimliği, tutorial ve diğer11 ada tanımı aynı. Beş10'luk grup ve tek yeniden kullanılan10 düğümlü bölüm seçicisi mevcut PrepareAsync/pin/Select/Gameplay üretici yoluna bağlıdır. Üretici, gameplay, kayıt, yıldız veya ödül kuralları değiştirilmedi.

## Gerçek testler

| Kontrol | Sonuç |
|---|---|
| PART13F canlı kabul | **1152 kontrol**, success=true,0 hata/0 uyarı |
|400→401 |400 gerçek Bottle UI çözüm hamleleriyle kazanıldı; Next401'i açtı; Easy ada girişi aynı |
|450→451 |450 gerçekten kazanıldı; Next451'i açtı;452 kilitli kaldı |
| Kilit/üretim |400 öncesi401 kilitli; kilitli tıklama üretici/arşiv/pin başlatmadı |
| Yıldız/kayıt/pin |401 yıldızı,402 açık/403 kilitli; Undo/Restart aynı düzen; diskten yıldız/signature aynı |
|50 gerçek kimlik | Beş grupta401–450; seçicide yalnız10 gerçek bölüm düğümü |
| Sandık |50/50 sonrası tek atomik ödül; tekrarı engelleme ve reload |
| Navigasyon/görev | Map→Home→günlük görev→etkinlik→Map; günlük/haftalık/sezon sayaç ve makbuzları, yıldız/sandık korundu |
| Havuz/release |120 hızlı0↔8 odak değişimi; aynı40 düğüm/5 görünüm,3 handle/instance; uzakta0 sanat |
| Mobil yerleşim |900×1600/1950/2000; son oranda dar Safe Area; metin taşması,44px altı hedef veya grup/sandık hit çakışması yok |
| Gerçek raycast |6 harita+10 bölüm merkezi; ilk GraphicRaycaster hedefleri doğru |
| Animasyon/fizik |3 ayrı dişlinin1.1sn aralıkta yavaş/ters yönlü açısal farkı ve enerji saati doğrulandı;0 Rigidbody/2D |
| Mevcut regresyon |1599 model+1425 Play Mode; oyun/güçler, ilk dört sprite, harita/kilit/ödül/sahne dönüşleri |
| Dil/derleme/Console |157 anahtar/8 dil; C# derleme temiz; son captured/groundTruth0 hata0 uyarı, compilationFailed=false |

Kanıtlar UNITY_TESTS.json, REGRESSION_*_TESTS.json, RAYCAST_*_TESTS.json, LOCALIZATION_SOURCE_TESTS.json, COMPILATION.json, CONSOLE.json. Sayılar assertion sayılarıdır; ayrı oyuncu oturumları değildir. Test benzersiz PlayerPrefs anahtarı ve geçici FileLevelArchive kullanır. Gerçek oyuncunun primary/backup/corrupt değerlerinin değişmediği doğrulandı. Önceki399/449 bölüm fixture'da tamamlanmış işaretlenir; hepsinin baştan insan gibi oynandığı iddia edilmez.400/401/450 gerçek UI hamleleriyle kazanıldı.

Part12BEditor sanat beklentisi8→9 güncellendi; diğer test kuralları aynı. Tarihî Part12B rapor/görselleri test öncesi kopyadan geri getirildi; yeni sonuçlar Part13F altındadır. İlk domain yenilemesinde UnityEditor.Overlays/ScriptableSingleton pencere seri hale getirme mesajları görüldü (INITIAL_EDITOR_DIAGNOSTICS.json); gameplay/C# derleme hatası değildi. Temiz kabul oturumunda tekrarlanmadı, son gerçek Console sayacı0'dır.

## Ölçülen performans — yalnız Windows Editor

PERFORMANCE_QUIET.json:60 ısınma+180 örnek; ölçüm sırasında dış Unity araç çağrısı yok. Medyan **7.65ms**, p95 **9.35ms**, maksimum **10.64ms**. Tüm Game View84 draw call/6374 üçgen,3 sanat handle/instance. Bu değerler telefon FPS/GPU/termal ölçümü veya yalnız adanın izole GPU maliyeti değildir.

REBIND_PERFORMANCE.json:120 zorlanmış yeniden bağlama215.456ms, ortalama1.795ms/olay; gerçek dokunmatik kaydırmanın kare başı maliyeti değildir. LOAD_PERFORMANCE.json: sıcak AssetDatabase Focus→instance0.34–41.17ms,1sn QA giriş beklemesi hariç. İlk FRAME_PERFORMANCE.json ham turunda1194ms uç değer korunur; nedeni ayrıştırılmadı ve oyun/cihaz performans kararı olarak kullanılmadı. Sessiz ısınmalı tur ayrıca alındı. Sıfır tahsis veya cihazda sabit60FPS iddiası yok.

ASSET_AUDIT/TEXTURE_BUDGET: dört doku Android/iPhone ASTC6×6, mipmap/ReadWrite kapalı; shader mesajı yok. Hesaplanan payload **1403424byte≈1.34MiB**; RGBA32 fallback12581520byte≈12.00MiB. Materyal/mesh/Addressables/diğer adalar/gökyüzü/overhead hariç. PNG disk boyutu ve Editor cache değeri cihaz RAM ölçümü değildir. Prefab PNG bağımlılıkları yalnız Clockwork klasöründe; dişliler merkez pivotlu, ortak sprite ve ayrı transformdur.

## Görsel teslim ve kalan sınırlar

LevelMap_GameView.png gerçek900×1600 native Game View screen kaydı. Clockwork_LargePreview.png gerçek1200×1400 Unity katman birleşimi;640 birimlik kök orantılı büyütülür. Clockwork_Map-*.png, Clockwork_Chapter441-*.png üç oran; Clockwork_Chapter.png, Clockwork_Locked.png, Clockwork_Motion_A/B.png, Gameplay_451.png, Daily_Missions.png, Events_Hub.png gerçek akış görüntüleridir. VISUAL_COMPARISON.html gerçek03_Map/büyük render/Game View karşılaştırmasıdır.

İncelemede taş/altın mekanik uçan ada, büyük saat kulesi, ışıklı atölyeler, mor-mavi enerji, yumuşak3D boyama ve görünür3 çark referansın masalsı premium diliyle uyumlu bulundu. Mevcut gece gökyüzü ve beş grup UI korunur;03_Map'in tekil yol düğümleri piksel kopyası değildir. İsim paneli alt kayanın bir kısmını örter; büyük önizleme tam silueti gösterir. Kadran/saat sembolleri illüstratif süstür; fiziksel saat doğruluğu veya gerçek dişli kinematiği iddia edilmez.

Fiziksel iPhone/Android dokunma/FPS/GPU/RAM/termal ve paketlenmiş Player/IL2CPP/Addressables bundle kabulü yapılmadı. Yeni build alınmadı. Bu sınırlar Editor başarısıyla karşılanmış sayılmaz.

## Koruma ve teslim

PRESERVATION.json2382 başlangıç tracked dosyasından yalnız7 ilgili dosyanın değiştiğini,2375 dosyanın byte olarak aynı olduğunu denetler. Önceki sekiz ada sanat/prefab/animasyonları ve Gameplay/Core/save/Pati/görev/event/localization üretim dosyaları aynı. Başlangıçtaki kirli TMP fallback gerçek byte yedeğinden geri getirildi ve SHA256 doğrulandı; teslim dışıdır. İlgisiz untracked çalışmalar korunur. Starlight Garden ve sonraki adalara başlanmadı.

Private edasu-yadik/mewgic-game hedefi isPrivate=true ile doğrulandı. Hedef branch recovery/part09-completion, master korunur. Editör Türkçe, izole401 başlangıç profiliyle Clockwork Game View'da bırakıldı. Git teslim kaydı push sonrası eklenir.

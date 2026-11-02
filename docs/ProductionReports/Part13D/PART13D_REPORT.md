# PART 13D — Coral Kingdom / Mercan Krallığı

2 Kasım 2026 · Unity6000.3.27f1 · recovery/part09-completion

## Sonuç ve kapsam

Yalnız yedinci ada, **301–350**, üretildi. AGENTS.md, PART13 mimarisi ve son13C kabulü incelendi. Önce kaliteli sanat üretildi; referanslar gerçek03_Map ve onaylı Crystal Falls oldu. Ekteki küçük örnek resim üretim girdisi olarak kullanılmadı. Yerleşik image_gen kullanıldı; Blender, CLI/API fallback, stok sanat veya nihai primitive ada yapılmadı.

Turkuaz su terasları üzerinde sedef/kabuk kaya kütlesi, ayrı inci sarayı, kristal su kemerleri, pembe-mor-altın mercan, deniz bitkileri, küçük şelaleler, düşük alpha huzmeler ve8 hafif kabarcık. Premium stilize mobil illüstrasyon hedeflendi; gerçekçi okyanus simülasyonu eklenmedi. **Yeni Player/iOS/IPA/Cloud/TestFlight veya Addressables içerik build'i başlatılmadı.**

Önceki PART13 ada sanat/prefab/animasyonları ve diğer yeni adalar korunur. Görev, etkinlik, kayıt, kilit, yıldız, ödül, üretici, Pati ve çoklu dil üretim kodu değiştirilmedi.

## Sanat kaynakları ve katmanlar

| Kaynak | İçerik | Runtime doku |
|---|---|---:|
| Assets/Art/Environment/Map/Islands/coral/IslandBase/Coral_Island_v001.png | Sedef ada tabanı, havuz/şelale terasları, yol ve küçük çevre ayrıntıları;1254² kaynak |1254² |
| …/Architecture/Coral_Palace_v001.png | Bağımsız inci/kabuk sarayı ve kristal su kemerleri;1254² kaynak |1024² |
| …/Vegetation/Coral_Cluster_v001.png | Bağımsız pembe/mor/altın mercan, parlayan deniz bitkisi ve inci/kabuk kümesi;1254² kaynak |512² |
| …/ForegroundEffects/Coral_Rays_v001.png | Ayrı yumuşak turkuaz ışık huzmesi;1536×1024 kaynak |512×341 |

Ada ve saray kaynak kopyaları `SourceArt/Map/Islands/coral/Renders/` altında da korunur. Importer sınırları yüksek çözünürlüklü PNG'leri küçültmez; kaynak boyutları2048 gibi sunulmadı. `ART_PLAN.md` kompozisyonu, `GENERATION_PROMPTS.md` kullanılan tam promptları kaydeder.

ALPHA_AUDIT.json: dört kaynakta da gerçek sıfır alpha ve yumuşak alpha kenarı var; alpha>8 olan tuval kenarı pikseli **0**. Saray/kaya uçları kadrajda. Büyük gerçek Unity renderında açık/mavi zemin üstünde birleşim, perspektif, ölçek ve alpha kenarı incelendi. Saray ayrı arka platforma, mercan bağımsız kıyı dekoruna oturtuldu; bütün harita tek resme çevrilmedi.

`Assets/Prefabs/Map/CoralKingdom.prefab` dokuz katman sırasını taşır: Background, IslandBase, Architecture, Vegetation, WaterOrSpecialEffects, Decorations, LevelNodes, Rewards, ForegroundEffects. Mimari ve büyük mercan kaynağı ayrı PNG/transform'dur. Küçük yol/kıyı ayrıntıları tabanda bake edilmiştir; her kabuk ayrı3D model değildir. Gerçek düğümler/sandık mevcut MapWorld sahibinde bağımsız UI olarak kalır.

## Su ve animasyon

`CoralIslandLayers.shader`: aynı kaynak atlasında turkuaz pikselleri tamamlayıcı maskeyle taban/mimari geçişinden çıkarır; ayrı WaterfallFlow/CrystalWaterArches bileşenleri aynı koordinatlarda bunları çizer. Su ile saray hizası korunur, ikinci büyük su dokusu gerekmez. Akış küçük zamanlı ışık bantlarıyla hareket eder. Nötr beyaz köpük/ince kenarlarda bazı statik pikseller kalır; bu gerçek su geometrisi/okyanus simülasyonu değildir.

`CoralIslandPresentation`:0.85sn hafif alpha/ölçek girişi; su/kemer materyal saati, düşük alpha inci ışığı, yaklaşık%2.3–4.7 opaklıkta ayrı huzme salınımı. `CoralBubbleGraphic`:8 küçük, yumuşak kenarlı halka; farklı fazlarda yaklaşık46 UI birimi yükselip alpha ile kaybolur. CanvasRenderer açıkça tanımlandı; tüm sanat/efekt raycastTarget=false, CanvasGroup blocksRaycasts=false. Yeni mekanik/tutorial veya ödül kuralı yok. Materyal örnekleri yok edilir; efektler yalnız aktif sanat instance'ında çalışır.

## Harita ve yükleme

Ortak IslandArtSlot'a yalnız `coral` desteği eklendi. `Map/coral`, ayrı yerel `Map-CoralKingdom` grubundaki tek prefab adresidir; Local.BuildPath/Local.LoadPath, PackTogether ve IncludeInBuild. Önceki ada/Localization grupları, uzak katalog ve normal gelecekteki Player build'inin Addressables ayarı değişmedi. Paketlenmiş Player/bundle kabulü yapılmadı; testler AssetDatabase Play Mode yolunu kullandı.

MapWorld'ün5 görünüm/40 MapPoint havuzu korunur. Görünür Coral ve komşu Woods/Mirage ile en fazla3 sanat handle/instance; görünür kümeden ayrılırken kapatma/yok etme ve release. Geç gelen yükleme nesil kontrolüyle iptal edilmiş slota bağlanmaz. Yeni sprite'lar terrain dizisine doğrudan eklenmedi;600 aktif bölüm GameObject'i oluşturulmadı.

World.json içindeki tek anlamsal fark **coral.artReady false→true**. Diğer11 ada tanımı,301–350 aralığı, ödül kimliği ve tutorial verileri aynıdır. Beş10'luk grup ve yeniden kullanılan10 gerçek bölüm düğümü mevcut PrepareAsync→doğrulanmış pin→Select→Gameplay yoluna bağlıdır. Yalnız Coral grup/sandık ankrajları ön kıyı yoluna uyarlandı; diğer ada yerleşimleri korunur.

## Gerçek testler

| Kontrol | Sonuç |
|---|---|
| PART13D canlı kabul | **1149 kontrol**, success=true,0 hata/0 uyarı |
|300→301 |300 gerçek Bottle UI hamleleriyle kazanıldı; Next gerçek301'i açtı, Easy giriş korundu |
|350→351 |350 gerçekten kazanıldı; Next351'i açtı;352 kilitli kaldı |
| Kilit |300 kazanılmadan301 kilitli; kilitli tıklama üretici/pin/oyun başlatmadı |
| Yıldız/pin/kayıt |301 kazanma,302 açık/303 kilitli; Undo/Restart aynı düzen; diskten yıldız ve seed/signature aynı |
|50 bölüm/sandık | Beş grupta gerçek301–350 kimlikleri;50/50 tamamlanınca tek atomik sandık, tekrar engeli ve reload |
| Navigasyon/görevler | Coral→Home→gerçek MissionsButton→DailyClose→NavEvents→RewardHubClose→NavMap; günlük progress/claimed/wins ve haftalık/sezon sayaç/makbuzları aynı |
| Dönüş | Home/görev/etkinlik sonrası Coral yıldızı ve alınmış sandık korundu |
| Havuz/bellek |120 hızlı yeniden bağlama; aynı40 düğüm/5 görünüm, en fazla3 sanat handle/instance; uzakta0 |
| Mobil oran |900×1600/1950/2000, daraltılmış Safe Area; taşma,44px altı hedef veya grup/sandık hit çakışması yok |
| Gerçek raycast |5 grup+1 sandık ve10 bölüm: **16 ilk GraphicRaycaster hedefi** |
| Mevcut harita regresyonu | **1599 model+1425 Play Mode**; ilk dört sprite, PART13 sanat yolu, kilit/ödül,601+, oyun/güçler ve sahne dönüşleri |
| Dil/derleme/Console |157 anahtar/8 dil hata yok; son C#/Console0 hata0 uyarı, compilationFailed=false |

Kanıtlar UNITY_TESTS.json, REGRESSION_*_TESTS.json, RAYCAST_*_TESTS.json, LOCALIZATION_SOURCE_TESTS.json, COMPILATION.json, CONSOLE.json. Assertion sayıları ayrı kullanıcı oturumları değildir.300/350 öncesindeki bölümler izole fikstürde tamamlanmış işaretlenir; önceki349 bölümün tamamı insan gibi oynanmış sayılmaz.300/301/350 gerçek UI çözüm hamleleriyle kazanıldı. Testler ayrı PlayerPrefs anahtarı/geçici FileLevelArchive kullandı; gerçek oyuncunun primary/backup/corrupt değerleri aynı.

Part12B model aracındaki sanat beklentisi6→7 güncellendi; diğer kontroller korunur. Tarihî PART12B rapor/görselleri test öncesi kopyalardan geri getirildi. Yeni kanıtlar yalnız PART13D altında. Yeni sanat entegrasyonunda düzeltme gerektiren derleme/runtime hata bulunmadı; son kabul araçları da temiz çalıştı.

## Performans ve bellek — yalnız Windows Editor

PERFORMANCE_QUIET.json:60 ısınma+180 gerçek kare, ölçüm sırasında dış Unity araç çağrısı yok. **Medyan9.84ms, p95 13.36ms, maksimum16.28ms**. Tüm Game View90 draw call/8338 üçgen; Coral prefabında7 Image+8 küçük kabarcık.3 görünür/komşu sanat handle ve3 aktif instance. Bunlar yalnız Coral'ın izole GPU maliyeti veya telefon FPS'i değildir.

REBIND_PERFORMANCE.json:120 zorlanmış yeniden bağlama toplam**245.316ms**, ortalama**2.044ms/olay**. Gerçek dokunmatik kaydırmanın kare başına maliyeti değildir. LOAD_PERFORMANCE.json: sıcak AssetDatabase oturumunda Focus→instance**0.33–33.78ms**, giriş animasyonunun1sn QA beklemesi hariç; telefon soğuk açılışı değildir.

İlk FRAME_PERFORMANCE.json turu QA aracı derlemesini de içeren1281ms maksimum ham değer taşıdığı için oyun performansı kararı olarak kullanılmadı; sessiz tur ayrıca alındı. Tahsis sıfır veya cihazda sabit60FPS iddia edilmedi.

ASSET_AUDIT.json/TEXTURE_BUDGET.json: dört doku Android/iPhone ASTC6×6, Read/Write kapalı, mipmap kapalı; shader mesajı yok. Hesaplanan doku payload'ı **1363520byte≈1.30MiB**, RGBA32 fallback yaklaşık11.66MiB. Atlas su geçişleriyle paylaşılır. Materyal/mesh/yükleyici, mevcut adalar ve gökyüzü bu hesaba dahil değil. Editor profiler değerleri kaynak/cache içerir, cihaz RAM'i değildir. PNG bağımlılıkları yalnız Coral klasöründedir; mimari/su/mercan ayrımı ve sanatta raycast engeli olmaması ayrıca doğrulandı.

## Görsel teslim ve kalite

- `LevelMap_GameView.png`: gerçek900×1600 Game View screen kaydı, Mercan Krallığı açık.
- `Coral_LargePreview.png`:1200×1400 büyük gerçek Unity renderı.
- `Coral_Map-1600/1950/2000.png`: üç mobil oran, izole1/50 tamamlanmış profil.
- `Coral_Chapter.png`, `Coral_Chapter341-*.png`: gerçek bağımsız bölüm seçicisi.
- `Coral_Locked.png`, `Coral_Motion_A/B.png`, `Gameplay_351.png`: kilit, farklı gerçek animasyon anları ve350 sonrası351 Gameplay.
- `Daily_Missions.png`, `Events_Hub.png`: gerçek görev/etkinlik regresyonu.
- `VISUAL_COMPARISON.html`:03_Map/büyük ada/gerçek harita yan yana; kaynaklar ve sınırlar açıklanır.

Görsel incelemede inci/kabuk mimarisi, turkuaz su, pembe-mor-altın mercan, yumuşak3D ayrıntı ve uçan kaya derinliği referansın premium stilize diliyle uyumlu bulundu. Mimari, mercan, su ve efektler ayrı bileşenler olarak gerçek sahnede birleştirildi; tek etkileşimsiz harita arka planı yapılmadı.

## Koruma ve kalan sınırlar

PRESERVATION.json:2231 başlangıç dosyasının yalnız7 ilgili dosyası (ilerleme belgesi dahil) değişti;2224 dosya byte olarak aynı. Woods/Mirage ve ilk dört adanın sanat/prefab/animasyonları; bütün Gameplay/Core/save/Pati/daily/event/localization üretim dosyaları aynı. Önceden kirli TMP fallback dosyası başlangıçta alınmış gerçek byte kopyasından geri getirildi ve SHA256 doğrulandı; commit edilmez. Diğer beş yeni ada üretilmedi.

Fiziksel iPhone/Android FPS/GPU/RAM/termal/dokunma ve paketlenmiş Player/IL2CPP kabulü yapılmadı. Su renk/konum maskeli2D akıştır; nötr köpükte bazı statik ayrıntılar kalır. Her küçük kabuk ayrı3D model değildir. Mevcut gece gökyüzü/paneller/beş grup düzeni korunur; isim plakası kayanın en altını kısmen örter. Genel UI,03_Map'in tekil düğümlü yolunun piksel kopyası olarak yeniden tasarlanmadı.

Private hedef `edasu-yadik/mewgic-game`, isPrivate=true ile doğrulandı (PRIVATE_REPOSITORY.json). Editör Türkçe, izole301 başlangıç profiliyle Coral Game View büyütülmüş halde bırakıldı. **Yeni build yok.** Git teslim kaydı aşağıya eklenir.

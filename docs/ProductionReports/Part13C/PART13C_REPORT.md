# PART 13C — Golden Mirage / Altın Serap

2 Kasım 2026 · Unity6000.3.27f1 · recovery/part09-completion

## Sonuç

Yalnız altıncı ada, **251–300**, üretildi. AGENTS.md, PART13A mimarisi ve PART13B kalite/kabul raporu incelendi. Sanat önce üretildi; onaylı03_Map ve Woods sprite'ı stil/kalite referansı oldu. Ekteki küçük örnek resim kaynak olarak kullanılmadı. Yerleşik image_gen kullanıldı; Blender/CLI/API fallback kullanılmadı.

Görsel kimlik: altın kumtaşı saray, sıcak gün batımı ışığı, kum terasları, turkuaz vaha ve iki küçük şelale, palmiye/egzotik çiçek, ayrı mor-mavi kristal kemer. Mevcut gökyüzü/diğer adalar değiştirilmedi; sıcaklık yalnız yeni adanın bake ışığında. Primitive geometri final sanat değildir.

**Yeni iOS/IPA/Player/Cloud/TestFlight build'i alınmadı.** Diğer yeni adalara geçilmedi. Oyun üreticisi, kayıt şeması, ödül kimlikleri, yıldız hesabı, çoklu dil ve Pati üretim kodu değişmedi.

## Üretilen varlıklar

- `Assets/Art/Environment/Map/Islands/mirage/IslandBase/Mirage_Island_v001.png`:1254×1254, gerçek RGBA alpha; kum/kaya, saray, palmiye ve küçük köprüler. Mimari/bitkiler statik sanata bake edildi.
- `.../Decorations/Mirage_CrystalArch_v001.png`: ayrı1254 kaynak, runtime512²; altın kemer, mor/mavi kristal ve çiçek kümesi. Bağımsız Image/RectTransform, taban resmine gömülmedi.
- `.../ForegroundEffects/Mirage_Mist_v001.png`: ayrı1254 kaynak, runtime512²; yaklaşık%3.5–5.5 alpha ile ince sis.
- `.../WaterOrSpecialEffects/Mirage_{Ground,Water,Sand,Lights}.mat` ve `Assets/Shaders/MirageIslandLayers.shader`: ayrı taban/su/kum/ışık geçişleri.
- `Assets/Prefabs/Map/GoldenMirage.prefab`: dokuz katman; ayrı dekor, su, sis, ışık ve8 düşük alpha kum parıltısı. Gerçek bölüm düğümleri ve sandık mevcut MapWorld sahibinde bağımsız kalır.
- `SourceArt/Map/Islands/mirage/Renders/Mirage_Island_v001.png`: seçilen kaynak kopyası. Diğer PNG'lerin yüksek çözünürlüklü kaynakları Assets içinde de korunur; importer sınırı kaynak dosyayı küçültmez.
- `ART_PLAN.md`, `GENERATION_PROMPTS.md`: kompozisyon/asset listesi ve kullanılan tam promptlar.

ALPHA_AUDIT.json: üç PNG'de gerçek sıfır alpha ve yumuşak alpha var; tuval kenarlarında alpha>8 piksel **0**. Kaynak ada sınırı[40,22,1233,1241]; kule ve kaya uçları kadraj içinde. Kaynak2048 gibi sunulmaz: araç çıktıları1254'tür. Gerçek Unity büyük önizlemesinde açık/mavi zeminde kenarlar ve su sonları kontrol edildi.

## Katman mimarisi ve entegrasyon

PART13A sırası: Background → IslandBase → Architecture → Vegetation → WaterOrSpecialEffects → Decorations → LevelNodes → Rewards → ForegroundEffects. Sabit mimari/bitki taban sanatıyla bake edilmiştir; dekor ve dinamik katmanlar bağımsızdır.

Su ve taban aynı atlası paylaşır, fakat aynı pikselleri üst üste boyamaz: shader turkuaz/alt bölge maskesini tabandan çıkarır; yalnız Water Image bu pikselleri çizer ve akış ışığını hareketlendirir. Böylece hizalama kaymaz, ikinci tam boy su dokusu gerektirmez. Nötr beyaz köpükte maskeye girmeyen küçük statik ayrıntılar tabanda kalır. Bu gerçek sıvı/geometri simülasyonu değildir. Ayrı Sand geçişi seyrek kum pırıltısı, Lights geçişi çok düşük alpha sıcak ışık nefesi üretir. Sis salınımı ve8 küçük glint ayrı bileşenlerdir.0.85sn hafif alpha/ölçek girişi korunur; yeni mekanik/tutorial eklenmez.

Ortak IslandArtSlot yalnız `mirage` adresini destekleyecek kadar genişletildi. Woods'un kendi prefab/sprite/materyal/shader/animasyon dosyaları değişmedi. Görünür slotta `Map/mirage` yerel Addressables prefabı yüklenir; görünür kümeden ayrılınca instance kapatılır/yok edilir ve handle release edilir. Geç gelen sonuç nesil kontrolüyle eski slota bağlanmaz. Woods+Mirage komşuluğunda en fazla2 handle/aktif instance tutulur; diğer bölgelerde sıfıra iner. terrain dizisine yeni doğrudan sanat referansı eklenmedi.

`Map-GoldenMirage` yerel grubunda tek prefab girdisi, PackTogether, IncludeInBuild, Local.BuildPath/Local.LoadPath. Localization/diğer ada grupları değişmedi, uzak katalog açılmadı. Mevcut normal Player build'inde Addressables içerik üretimi açıktır; bu görevde paketlenmiş Player/bundle kabulü yapılmadı.

World.json içindeki tek anlamsal değişiklik **mirage.artReady false→true**. İlk beş ve diğer altı ada tanımı,50'lik aralıklar, reward/tutorial alanları aynıdır. MapWorld'ün5 ada görünümü/40 MapPoint havuzu, beş10'luk grup ve tek10 bölüm seçicisi korunur;600 aktif bölüm objesi yaratılmaz.251–300 mevcut PrepareAsync→sertifikalı pin→Select→Gameplay yoluna bağlıdır. Yalnız Mirage'ın grup/sandık ankrajları terasına uyarlandı; diğer ada yerleşimleri korunur.

## Gerçek testler

| Kontrol | Sonuç |
|---|---|
| PART13C canlı Play Mode | **1050 kontrol**, success=true,0 hata/0 uyarı |
|250→251 |250 gerçek Bottle UI hamleleriyle kazanıldı; Next gerçek251'i açtı, Easy giriş korundu |
|300→301 |300 gerçek kazanıldı; Next301'i açtı;302 kilitli kaldı |
| Kilit |250 kazanılmadan251 kilitli; kilitli tıklama pin/oyun üretmedi |
| Yıldız/pin/kayıt |251 kazanıldı,252 açıldı; Undo/Restart aynı pin; FileArchive/PlayerPrefs yeniden açılışında yıldız ve seed/signature aynı |
|50 bölüm/sandık | Beş grup gerçek251–300 kimlikleri;50/50 tamamlanmada sandık, tek atomik ödül, tekrar engeli ve reload geçti |
| Kaydırma/bellek |120 hızlı yeniden bağlama; aynı40 düğüm/5 ada görünümü, en fazla2 sanat handle/instance; görünmezken0 |
| Mobil yerleşim |900×1600,900×1950,900×2000 ve daraltılmış Safe Area; metin taşması yok, hedefler≥44px, grup/sandık hit çakışması yok |
| Gerçek raycast |5 grup+1 sandık ve10 bölüm için **16 ilk GraphicRaycaster hedefi** geçti |
| Mevcut harita regresyonu | **1599 model+1425 Play Mode** geçti; ilk dört sprite, Woods, kilit/ödül,601+, gerçek oyuna giriş/dönüş ve mekanik model kontrolleri |
| Dil kaynağı |157 anahtar/8 dil; hata yok, yeni metin eklenmedi |
| Son derleme/Console |0 hata/0 uyarı; compilationFailed=false |

Kanıtlar UNITY_TESTS.json, REGRESSION_MODEL_TESTS.json, REGRESSION_UNITY_TESTS.json, RAYCAST_*_TESTS.json, LOCALIZATION_SOURCE_TESTS.json, COMPILATION.json, CONSOLE.json. Assertion sayıları ayrı oyuncu oturumları değildir.250 ve300 öncesi bölümler izole fikstürde tamamlanmış işaretlenir; önceki299 bölümün tamamı insan gibi oynanmış sayılmaz. Sınır bölümleri250/251/300 gerçek UI hamleleriyle kazanıldı. Ana oyuncunun primary/backup/corrupt değerleri aynı; testler ayrı PlayerPrefs anahtarı ve geçici FileLevelArchive kullandı.

Eski Part12B model aracının sanat beklentisi5→6 olarak güncellendi; diğer kontroller korunur. Tarihî PART12B kanıtları önceden alınan kopyalardan geri getirildi, yeni çıktılar yalnız PART13C altında tutuldu. Asset denetim aracındaki eski adres beklentisi ve sessiz ölçüm aracındaki callback türü düzeltildi; son araç derlemeleri/çalıştırmaları temiz geçti. Bunlar üretim hata sonucu diye veya başarılı ilk tur diye sunulmadı.

## Performans — yalnız Windows Editor

PERFORMANCE_QUIET.json:60 ısınma karesi+180 ölçülen gerçek kare. Bu sırada dış Unity araç çağrısı yapılmadı. **Medyan9.17ms, p95 11.83ms, maksimum13.52ms**, tüm Game View için86 draw call/6630 üçgen; Mirage prefabında6 Image ve8 küçük glint. Görünür Woods ile birlikte2 handle/2 aktif sanat instance'ı. Bu telefon FPS/GPU/termal testi veya yalnız Mirage'ın izole draw call sayısı değildir.

REBIND_PERFORMANCE.json:120 zorlanmış yeniden bağlama toplam**236.714ms**, ortalama**1.973ms/olay**; Addressables/instance işlemlerini içeren Editor stres turu. Bu gerçek dokunmatik kaydırmanın kare başına maliyeti değildir. LOAD_PERFORMANCE.json: sıcak AssetDatabase oturumunda Focus→instance örnekleri**0.28–30.98ms**, giriş animasyonunun1sn QA beklemesi hariç; telefon soğuk açılışı değildir.

İlk FRAME_PERFORMANCE.json turu sırasında QA aracı derlemesi de yapıldı;1287.6ms maksimum ham değer saklandı, oyun performansı kararı için kullanılmadı. Yukarıdaki sessiz tur bu nedenle ayrıca alındı. Tahsis sıfır veya cihazda sabit60FPS iddia edilmez.

ASSET_AUDIT.json: üç doku Android/iPhone ASTC6×6, Read/Write kapalı, mipmap kapalı.1254² taban+iki512² katman için blok payload hesabı**935568byte≈0.89MiB**; RGBA32 fallback yaklaşık8MiB. Ortak atlas su için tekrar doku belleği üretmez. Materyal/mesh/yükleyici, gökyüzü ve diğer adalar bu hesaba dahil değil. Editor profiler değerleri kaynak/cache içerir, cihaz RAM'i değildir. Shader mesajı yok, sanat raycast'i kapalı, bağımsız dekor/taban/su modları ve yalnız Mirage PNG bağımlılıkları doğrulandı.

## Görsel teslim ve referans kabulü

- LevelMap_GameView.png: gerçek900×1600 Game View screen kaydı, Altın Serap açık.
- Mirage_LargePreview.png:1200×1400 büyük gerçek Unity renderı.
- Mirage_Map-1600/1950/2000.png: üç mobil oran; izole1/50 tamamlanmış profil.
- Mirage_Chapter.png ve Mirage_Chapter291-*.png: gerçek bağımsız bölüm seçicileri.
- Mirage_Locked.png:251 açılmadan kilit durumu.
- Mirage_Motion_A/B.png: farklı gerçek animasyon anları; shader saati/sis/parıltı değişimi ayrıca sınandı.
- Gameplay_301.png:300 sonrası gerçek301 Gameplay.
- VISUAL_COMPARISON.html:03_Map, büyük ada ve gerçek harita yan yana.

Görsel incelemede saray ve altın kumun Woods'tan farklı silueti/paleti, yumuşak3D ayrıntı, turkuaz su, sıcak pencereler ve uçan kaya derinliği referans kalitesiyle uyumlu bulundu. Ayrı kemer/kristal dekoru sahnede seçilebilir/düzenlenebilir. Bütün haritayı tek etkileşimsiz resme dönüştürme yapılmadı.

## Kalan sınırlar ve koruma

Fiziksel iPhone/Android dokunma/FPS/RAM/termal ve paketlenmiş Player/IL2CPP testleri yapılmadı. Gün batımı yeni adanın bake ışığında; mevcut gece gökyüzü korunur. Saray/bitkilerin her biri ayrı düzenlenebilir3D model değildir. Su efekti renk/konum maskeli2D akış/parıltıdır; nötr köpükte bazı statik pikseller kalır. Mevcut haritanın beş grup seçicisi, panelleri ve ada aralıkları03_Map'in tekil düğümlü yolundan farklıdır; genel UI yeniden tasarlanmadı.

PRESERVATION.json: başlangıçtaki2162 takipli dosyanın yalnız yedi ilgili dosyası (ilerleme belgesi dahil) değişti;2155 dosya byte olarak aynı. Mevcut beş adanın sanat/prefab/animasyon dosyaları, bütün Gameplay/Core/save/Pati/localization üretim dosyaları korundu. Önceden kirli olan TMP fallback dosyası başlangıçtaki gerçek byte kopyasından geri getirildi ve SHA256 doğrulandı; commit kapsamı dışında. Diğer altı yeni adanın üretimine başlanmadı.

Editör Türkçe, izole251 başlangıç profiliyle Altın Serap Game View büyütülmüş halde bırakıldı. Yeni TestFlight build'i yok. Git teslim kaydı aşağıya eklenir.

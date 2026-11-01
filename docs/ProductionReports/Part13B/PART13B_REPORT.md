# PART 13B — Whispering Woods / Fısıltılı Orman

2 Kasım 2026 · Unity 6000.3.27f1 · recovery/part09-completion

## Sonuç ve kapsam

Yalnız beşinci ada (201–250) üretildi. `03_Map.png`, onaylı Moon/Village/Crystal/Castle sanatları, AGENTS.md ve PART13A raporu incelendi. Ekteki küçük örnek fotoğraf üretim referansı olarak kullanılmadı. Önce kompozisyon ve asset listesi ART_PLAN.md ile yazıldı; ardından yerleşik image_gen ile ada ve ayrı sis sprite'ı üretildi. Blender renderı bu 2D üretim yolunda gerekli olmadı; yapılmış gibi gösterilmez. Yeni mekanik veya tutorial yok. **Yeni iOS/IPA/Player/Cloud/TestFlight build'i alınmadı.**

İlk dört ada sprite/importer/prefab referansları, Gameplay, üretici, kayıt model/şeması, ödül kimlikleri, çoklu dil ve Pati üretim kodu korunur. Diğer yeni adaların artReady değerleri kapalı kaldı. `World.json` içindeki tek anlamsal değişiklik woods.artReady false→true; artIndex=-1 kalır, katmanlı sanat `artKey=woods` üzerinden çözülür.

## Sanat teslimi ve kalite

- `Assets/Art/Environment/Map/Islands/woods/IslandBase/Woods_Island_v002.png`: gerçek transparan 1254×1254 PNG. Dev mor/mavi ağaçlar, altın pencereli ahşap evler, ince asma köprüler, altın mantarlar, çiçekler, kaya alt kütlesi ve turkuaz şelaleler. UI/düğüm/sandık resme gömülmedi.
- `.../ForegroundEffects/Woods_Mist_v001.png`: ayrı transparan sis. Unity runtime'da 1024²; düşük alpha ile hareket eder.
- `SourceArt/Map/Islands/woods/Renders/Woods_Island_v002.png`: seçilen kaynak sanatın değişmez kopyası. Blender kaynağı üretilmedi. İlk kadrajı kırpık varyant yerine, taç/alt uç bütünüyle kadrajda kalan v002 kullanıldı.
- `GENERATION_PROMPTS.md`: tam üretim/düzeltme promptları ve yöntem. Araç 2048 isteğine rağmen 1254 kaynak verdi; dosyalar 2048 ayrıntısı varmış gibi sunulmaz veya yapay olarak büyütülmez.
- `ALPHA_AUDIT.json`: iki kaynakta da gerçek sıfır alpha, yumuşak alpha kenarı; >8 alpha olan tuval kenarı pikseli 0. Ada alpha sınırı [64,48,1213,1176]. Büyük Unity önizlemesinde açık/mavi gökyüzü üstünde kenar ve şelale sonları incelendi.

Referansla karşılaştırma `VISUAL_COMPARISON.html`: zengin mor/mavi, sıcak altın pencere, yumuşak 3D illüstrasyon, katmanlı yaprak/kaya ayrıntısı, uçan ada derinliği, turkuaz su ve çiçek dili ilk dört adaya uyumlu bulundu. Büyük önizleme gerçek Unity renderıdır. Mevcut harita grup seçicisi, büyük panelleri ve boşlukları 03_Map'in tekil bölüm yolunun piksel kopyası değildir. Bu part ilk dört adayı veya genel UI düzenini yeniden tasarlamaz.

## Katmanlar ve çalışma zamanı

`Assets/Prefabs/Map/WhisperingWoods.prefab` PART13A'nın dokuz katman sırasını taşır: Background, IslandBase, Architecture, Vegetation, WaterOrSpecialEffects, Decorations, LevelNodes, Rewards, ForegroundEffects. Statik taban/mimari/bitki/dekor ana PNG'ye bake edildi; dokuz ayrı tam boy doku oluşturulmadı. Bağımsız düzenlenebilir 3D orman modeli iddia edilmez.

- MapWorld'ün beş ada / kırk MapPoint havuzu korunur. Mevcut beş adet 10'luk grup ve tek 10 bölüm seçicisi 201–250 gerçek kimliklerine bağlanır. 600 GameObject veya 50 yeni düğüm yaratılmaz. Bölüm girişi mevcut asenkron PrepareAsync→doğrulanmış pin→Select→Gameplay yoludur.
- `IslandArtSlot`: yalnız görünür woods slotunda `Map/woods` adresi yüklenir. Başka bölgeye/pakete geçişte instance kapatılır/yok edilir, yükleme handle'ı tam bir kez release edilir. Geç gelen sonuç nesil kontrolüyle iptal edilmiş slota bağlanmaz. Diğer dört sprite terrain dizisinde aynı kalır; yeni sanat bu diziye topluca eklenmez. En fazla bir Woods handle/instance tutulur.
- Yerel `Map-WhisperingWoods` Addressables grubunda tek prefab girdisi, PackTogether, Local.BuildPath/Local.LoadPath, IncludeInBuild. Localization grupları değişmedi; uzak katalog açılmadı. Normal gelecekteki Player build'inin Addressables içerik üretimi mevcut ayarla açıktır. Bu görevde asset database Play Mode yolu doğrulandı; paketlenmiş Player/IL2CPP testi yapılmadı.
- Gerçek düğümler ve sandık mevcut MapWorld/MapNodeBadge/Button bileşenleridir; görsel prefabın alpha açılışından bağımsızdır. Sanat/efekt Graphic raycastTarget=false ve CanvasGroup blocksRaycasts=false. Düğüm satırı bu adanın ön terasına göre ayarlandı; sandık son düğümün dokunma alanından ayrıldı.
- `WoodsIslandPresentation`: 0.85sn cubic ease opacity/ölçek .975→1 giriş; su/ışık materyal örnekleri, ince sis salınımı ve 12 küçük yumuşak altın ateşböceği. Giriş oyun kaydına yeni eğitim/mekanik işareti eklemez. Animasyon yalnız görünür instance'da çalışır. Materyal örnekleri yok edilir.
- `WoodsIslandEffects.shader`: ayrı WaterfallFlow Image, kaynağın alt bölümündeki turkuaz su piksellerini seçer; aşağı akan ışık bantları. Mavi ağaç tepeleri su maskesine girmez. Ayrı GoldenWindowBreathing katmanı sıcak pencere/mantar renklerinde düşük alpha ışık nefesi sağlar. Bu gerçek sıvı simülasyonu veya geometri animasyonu değildir; mobil 2D efektidir. RectMask2D/stencil kırpması korunur.

## Bellek ve performans sınırı

İki doku Read/Write kapalı, mipmap kapalı, bilinear/clamp. Android/iPhone ASTC6×6 override. Kaynak ada1254² ve runtime sis1024² için ASTC blok hesabı: 698896 +467856 =1166752 byte, yaklaşık **1.11 MiB** doku payload'ı. Sıkıştırmasız RGBA fallback yaklaşık **10 MiB**. UI/shared gökyüzü, eski dört resident ada, materyal/mesh/Addressables yükleyici overhead'i bu hesaba dahil değildir.

`ASSET_AUDIT.json` Editor profiler sayıları ada için 7187856 byte, sis468752 byte gösterir; Editor kaynak/asset cache maliyeti içerdiğinden telefondaki RAM gibi sunulmaz. Shader mesajı yok, yerel grup/dependency zinciri ve sanatta raycast engeli yok. Hızlı kaydırmada handle/instance sınırı ve görünmeyince sıfıra iniş gerçek testte ölçüldü. Fiziksel iPhone/Android FPS/GPU/RAM/termal/dokunma ölçümü yapılmadı.

## Test kapsamı

PART13B izole PlayerPrefs + geçici FileLevelArchive kullanır. 200 ve 250 öncesindeki eski bölümler test fikstüründe tamamlanmış olarak işaretlenir; önceki 249 bölümün insan gibi oynandığı iddia edilmez. Sınırdaki 200,201 ve250 bölümleri gerçek UI Bottle düğmeleri ve üreticinin çözüm sertifikasıyla kazanılır; Next ile201 ve251 gerçekten açılır. Yıldız, Undo/Restart, kilit, 50/50 sandık, tek ödül, yeniden açılışta aynı pin, 120 hızlı havuz değişimi, hareket durumları ve mobil Safe Area test edilir.

Mevcut Part12B model/Play Mode araçlarının yalnız 'beşinci ada sanatı bekliyor' beklentisi güncellendi; diğer kontroller korundu. Yeniden çalıştırılan eski harita regresyonu sonuçları `REGRESSION_MODEL_TESTS.json` (1599) ve `REGRESSION_UNITY_TESTS.json` (1425) altında. İlk dört sprite, kilitler,601+ paket, oyuna giriş/geri dönüş, Undo/Restart/Pause, yıldız, eski sandık göçü ve gerçek oyuncu kayıtları geçti. Bu çalıştırma mekanik model regresyonunu da içerir. Tarihî PART12B rapor/görselleri test öncesi kopyalarından geri getirildi; yeni kanıtlar yalnız PART13B altında.

## Görseller

- `Woods_LargePreview.png`:1200×1400 büyük gerçek Unity ada önizlemesi.
- `Woods_Map-1600/1950/2000.png`:900 genişlikte gerçek çalışan LevelMap renderları;2000 örneğinde daraltılmış Safe Area. 1/50 tamamlanmış test profili.
- `Woods_Chapter.png`, `Woods_Chapter241-*.png`: bağımsız etkileşimli gerçek bölüm seçicisi.
- `Woods_Locked.png`:200 kazanılmadan kilitli201.
- `Woods_Motion_A/B.png`: iki farklı gerçek animasyon anı. Shader saati/sis konumu/ateşböceği alpha değişimi ayrıca test edilir.
- `Gameplay_251.png`:250 sonrası gerçek251 Gameplay.
- `LevelMap_GameView.png`: gerçek Game penceresinin screen kaydı; Editor pencere ölçeği/kenar boşluğu içerebilir. Tam çözünürlüklü kalite incelemesi üstteki renderlar ve büyük önizleme üzerinden yapıldı.

## Koruma ve düzeltilenler

İlk canlı denemede özel ateşböceği Graphic için CanvasRenderer eksikliği yakalandı; RequireComponent ve prefab üretimine açık CanvasRenderer eklendi, ardından temiz testler yeniden çalıştırıldı. QA aracındaki Editor assembly çağrısı düzeltildi. İlk açılışta Editor arka planda kare ilerletmedi; yalnız QA oturumunda runInBackground açılarak gerçek kare ilerlemesi doğrulandı. Bu ilk denemeler son başarılı test sonucu yerine sunulmaz.

TMP, kurulumdaki SaveAssets sırasında dinamik fallback atlasına U+25BC ekledi. Önceden de kirli olan fallback dosyası görev başındaki SHA256 ile birebir eşleşen byte içeriğine geri getirildi; eski kullanıcı whitespace değişikliği korundu. `FONT_PRESERVATION.json` ve `PRESERVATION.json` bunu doğrular. Font dosyası commit edilmez.

Diğer yedi ada için üretim yapılmadı. Önceki hazırlıktaki `library` klasör adı mevcut genel Library ignore kuralına takılıyor; bu part onun dosyalarına müdahale etmez. İlgili adanın ilerideki üretiminde Git kapsamı ayrıca ele alınmalıdır.

## Son kabul

- UNITY_TESTS.json: **1049 kontrol geçti**, errors=[], warnings=[], playerSaveAndBackupIntact=true. Sandık konumunun son düzenlenmesinden sonra tam tur yeniden alındı.
- REGRESSION_MODEL_TESTS.json: **1599**; REGRESSION_UNITY_TESTS.json: **1425** kontrol geçti.
- RAYCAST_MAP_TESTS.json: beş grup + sandık **6 gerçek GraphicRaycaster hedefi**; RAYCAST_CHAPTER_TESTS.json: **10 gerçek bölüm hedefi** geçti. Panelin çizilen sonraki karesinde ve gerçek900×1600 Game View üzerinde ölçüldü. Aynı karede/açık Scene View boyutlarıyla yapılan ilk QA denemeleri kabul sayılmadı; oyun kodunda bu yüzden bir düzeltme gerekmedi.
- Üç oran900×1600/1950/2000 ve daraltılmış Safe Area'da metin taşması, 44px altı hedef ve Woods grup/sandık hit alanı çakışması yok.
- COMPILATION.json:0 hata/0 uyarı; CONSOLE.json son çalışma durumunda0 hata/0 uyarı, compilationFailed=false.
- LOCALIZATION_SOURCE_TESTS.json:157 anahtar/8 dil, hata yok. Yeni kullanıcı metni veya çeviri eklenmedi.
- PRESERVATION.json: yalnız yedi ilgili mevcut dosya (ilerleme belgesi dahil) değişti; diğer2100 başlangıç dosyasından2093'ü byte olarak aynı. Başlangıçtaki ilgisiz font farkı birebir korundu. İlk dört ada/sahne, oyun modeli, üretici, kayıt ve Pati üretim dosyaları değişmedi.

Editör Türkçe, izole201 başlangıç profiliyle LevelMap Game View büyütülmüş halde bırakıldı. Gerçek oyuncunun ilerlemesi değiştirilmedi. Teknik/görsel Editor kabulü tamamlandı; fiziksel cihaz ve paketlenmiş Player kabulü bu raporun dışında, yeni iOS build'i yok.

## Git teslimi

Uygulama ve kabul commit'i `7b232812f2a66f6ae1f0bd415a8c4983acf29fb3`. `git push origin recovery/part09-completion` başarılı; `git ls-remote` uzak dalın aynı SHA olduğunu doğruladı. Master `f243d7a355fedf9cfbc65f8060f1d3bc14cae169` olarak değişmedi. Yalnız69 ilgili dosya (sanat, prefab/yerel grup, yükleyici/efekt, kabul araçları ve kanıtlar) gönderildi. Önceden var olan font ve diğer ilgisiz dosyalar commit kapsamı dışında kaldı. Bu teslim kaydı ayrı dokümantasyon commit'iyle gönderilir. **Yalnız PART13B tamamlandı.**
